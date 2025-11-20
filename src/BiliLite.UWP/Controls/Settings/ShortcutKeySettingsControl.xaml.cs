using AutoMapper;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Models.Functions;
using BiliLite.Services;
using BiliLite.ViewModels.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls.Settings
{
    public sealed partial class ShortcutKeySettingsControl : UserControl
    {
        private readonly ShortcutKeySettingsControlViewModel m_viewModel;
        private readonly ShortcutKeyService m_shortcutKeyService;
        private readonly IMapper m_mapper;
        private ShortcutFunctionViewModel m_recordingKeysShortcutFunction;
        private bool m_isRecording;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private bool m_loaded = false;

        public ShortcutKeySettingsControl()
        {
            m_mapper = App.ServiceProvider.GetRequiredService<IMapper>();
            m_shortcutKeyService = App.ServiceProvider.GetRequiredService<ShortcutKeyService>();
            m_viewModel = App.ServiceProvider.GetRequiredService<ShortcutKeySettingsControlViewModel>();
            m_viewModel.ShortcutFunctions = m_mapper.Map<ObservableCollection<ShortcutFunctionViewModel>>(m_shortcutKeyService.ShortcutFunctions);
            m_viewModel.PressActionDelayTime = m_shortcutKeyService.PressActionDelayTime;
            m_viewModel.PositionMoveLength = SettingService.GetValue(SettingConstants.ShortcutKey.POSITION_MOVE_LENGTH,
                SettingConstants.ShortcutKey.DEFAULT_POSITION_MOVE_LENGTH);
            DataContext = m_viewModel;
            InitializeComponent();

            m_shortcutKeyService.OnRecordKeyDown += ShortcutKeyService_OnRecordKeyDown;
            m_shortcutKeyService.OnRecordStoped += ShortcutKeyService_OnRecordStoped;

            // 鼠标中键/侧键行为
            cbMouseMiddleAction.SelectedIndex = SettingService.GetValue(SettingConstants.UI.MOUSE_MIDDLE_ACTION, (int)MouseMiddleActions.Back);
            cbMouseMiddleAction.Loaded += new RoutedEventHandler((sender, e) =>
            {
                cbMouseMiddleAction.SelectionChanged += new SelectionChangedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.MOUSE_MIDDLE_ACTION, cbMouseMiddleAction.SelectedIndex);
                });
            });

            Loaded += ShortcutKeySettingsControl_Loaded;
        }

        private void ShortcutKeySettingsControl_Loaded(object sender, RoutedEventArgs e)
        {
            m_loaded = true;
        }

        private async void ShortcutKeyService_OnRecordStoped(object sender, System.EventArgs e)
        {
            // 延迟一段时间避免重复进入录制
            await Task.Delay(50);
            m_isRecording = false;
        }

        private void UpdateShortcutFunctions(ShortcutFunctionViewModel viewModel)
        {
            var shortcutFunction = m_mapper.Map<ShortcutFunctionModel>(viewModel);
            m_shortcutKeyService.UpdateShortcutFunction(shortcutFunction);
        }

        private void ShortcutKeyService_OnRecordKeyDown(object sender, InputKey e)
        {
            if (m_recordingKeysShortcutFunction == null)
            {
                _logger.Warn("m_recordingKeysShortcutFunction is null");
                return;
            }

            if (m_recordingKeysShortcutFunction.Keys == null)
            {
                _logger.Warn("m_recordingKeysShortcutFunction.Keys is null");
                m_recordingKeysShortcutFunction.Keys = new ObservableCollection<InputKey>();
            }
            m_recordingKeysShortcutFunction.Keys.Add(e);
            m_recordingKeysShortcutFunction.UpdateKeysString();
            UpdateShortcutFunctions(m_recordingKeysShortcutFunction);
        }

        private void BtnRecordKeys_OnClick(object sender, RoutedEventArgs e)
        {
            if (m_isRecording) return;
            if (!(sender is Button { DataContext: ShortcutFunctionViewModel shortcutFunction }))
            {
                return;
            }

            m_recordingKeysShortcutFunction = shortcutFunction;
            m_recordingKeysShortcutFunction.Keys = new ObservableCollection<InputKey>();
            m_shortcutKeyService.StartRecord();
            m_isRecording = true;
        }

        private void NumberBoxPressActionDelayTime_OnValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            m_shortcutKeyService.PressActionDelayTime = (int)sender.Value;
        }

        private async void ShortcutFunctionViewModel_Changed<T>(object sender, T e)
        {
            //TODO:  恢复默认过程中也应该停止写入设置
            if (!m_loaded) return;

            // 等ViewModel实际更新
            await Task.Delay(50);
            if (sender is FrameworkElement { DataContext: ShortcutFunctionViewModel viewModel })
            {
                UpdateShortcutFunctions(viewModel);
            }
        }

        private void BtnDeleteShortcutFunction_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: ShortcutFunctionViewModel viewModel })
            {
                m_viewModel.ShortcutFunctions.Remove(viewModel);
                m_shortcutKeyService.RemoveShortcutFunction(viewModel.Id);
            }
        }

        private void BtnSetDefault_OnClick(object sender, RoutedEventArgs e)
        {
            m_shortcutKeyService.SetDefault();
            m_viewModel.ShortcutFunctions = m_mapper.Map<ObservableCollection<ShortcutFunctionViewModel>>(m_shortcutKeyService.ShortcutFunctions);
        }

        private async void BtnAddAction_OnClick(object sender, RoutedEventArgs e)
        {
            m_viewModel.AddShortcutFunctionModel = m_mapper.Map<ShortcutFunctionViewModel>(DefaultShortcuts.GetDefaultShortcutFunctions().First());
            m_viewModel.AddShortcutFunctionTypeName = m_viewModel.AddShortcutFunctionModel.TypeName;
            await NotificationShowExtensions.ShowContentDialog(AddActionDialog);
        }

        private async void ComboAddAction_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await Task.Delay(50);
            var typeName = m_viewModel.AddShortcutFunctionTypeName;
            var functionModel = m_mapper.Map<ShortcutFunctionViewModel>(DefaultShortcuts.GetDefaultShortcutFunctions()
                .FirstOrDefault(x => x.TypeName == typeName));
            if (functionModel == null) return;
            m_viewModel.AddShortcutFunctionModel = functionModel;
        }

        private void AddActionDialog_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            m_viewModel.ShortcutFunctions.Add(m_viewModel.AddShortcutFunctionModel);
            var shortcutFunction = m_mapper.Map<ShortcutFunctionModel>(m_viewModel.AddShortcutFunctionModel);
            m_shortcutKeyService.AddShortcutFunction(shortcutFunction);
        }

        private async void PositionMoveLength_OnValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            await Task.Delay(50);
            SettingService.SetValue(SettingConstants.ShortcutKey.POSITION_MOVE_LENGTH,
                m_viewModel.PositionMoveLength);
        }
    }
}
