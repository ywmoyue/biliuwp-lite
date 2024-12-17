param (
    [string]$csprojPath
)

# Check if the input file exists
if (-not (Test-Path $csprojPath)) {
    Write-Error "File not found: $csprojPath"
    exit 1
}

# Read the content of the csproj file with UTF-8 encoding
$csprojContent = Get-Content -Path $csprojPath -Raw -Encoding UTF8

# Define the target PropertyGroup condition
# Use backticks to escape the $ character
$targetCondition = "'`$(Configuration)|`$(Platform)' == 'Release|x64'"

# Find the PropertyGroup with the target condition
$propertyGroupRegex = [regex]::Escape("<PropertyGroup Condition=`"$targetCondition`">") + ".*?" + [regex]::Escape("</PropertyGroup>")
$propertyGroupMatch = [regex]::Match($csprojContent, $propertyGroupRegex, [Text.RegularExpressions.RegexOptions]::Singleline)

if ($propertyGroupMatch.Success) {
    # Extract the PropertyGroup content
    $propertyGroupContent = $propertyGroupMatch.Value

    # Check if UseDotNetNativeToolchain exists in the PropertyGroup
    $useDotNetNativeToolchainRegex = [regex]::Escape("<UseDotNetNativeToolchain>") + ".*?" + [regex]::Escape("</UseDotNetNativeToolchain>")
    $useDotNetNativeToolchainMatch = [regex]::Match($propertyGroupContent, $useDotNetNativeToolchainRegex)

    if ($useDotNetNativeToolchainMatch.Success) {
        # Replace the value of UseDotNetNativeToolchain with true
        $updatedPropertyGroupContent = $propertyGroupContent -replace $useDotNetNativeToolchainRegex, "<UseDotNetNativeToolchain>true</UseDotNetNativeToolchain>"
    } else {
        # If UseDotNetNativeToolchain doesn't exist, add it
        $updatedPropertyGroupContent = $propertyGroupContent -replace "</PropertyGroup>", "<UseDotNetNativeToolchain>true</UseDotNetNativeToolchain></PropertyGroup>"
    }

    # Replace the original PropertyGroup with the updated one
    $csprojContent = $csprojContent -replace [regex]::Escape($propertyGroupContent), $updatedPropertyGroupContent

    # Write the updated content back to the file with UTF-8 encoding
    Set-Content -Path $csprojPath -Value $csprojContent -Encoding UTF8

    Write-Host "Updated UseDotNetNativeToolchain to true in the PropertyGroup with condition '$targetCondition'."
} else {
    Write-Warning "PropertyGroup with condition '$targetCondition' not found in the csproj file."
}