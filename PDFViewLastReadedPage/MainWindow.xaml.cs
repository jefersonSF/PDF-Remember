using Common;
using Domain;
using Microsoft.Win32;
using Models;
using PdfiumViewer;
using PdfiumViewer.Core;
using PDFViewLastReadedPage.Configuration;
using Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PDFViewLastReadedPage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WindowSizeSettings _windowSizeSettings;
        private bool _spacePressing;
        private bool _mouseDragging;
        private System.Windows.Point _lastPositionMouse;
        private int _totalPages;
        private int _currentPage;
        private PdfInfo _pdfInfo;
        private PdfInfoServices _pdfInfoServices;
        private RecentServices _recentServices;
        private PreferenceServices _preferenceServices;
        private ICollection<Preference> _preferences;

        const float _zoomInitial = 0.70f;
        public MainWindow(string? filePath)
        {            
            _pdfInfoServices = new PdfInfoServices();
            _recentServices = new RecentServices();
            _preferenceServices = new PreferenceServices();
            _windowSizeSettings = new WindowSizeSettings(this);            
            this.Initialized += async (s, e) =>
            {
                //this.WindowState = WindowState.Maximized;                
                await LoadPreferences();
                await LoadMenus();
            };
            this.Loaded +=async (s, e) =>
            {
                if (!string.IsNullOrEmpty(filePath))
                {
                    await LoadPDF(filePath);
                }
                //Primeiro centraliza a janela principal, pra garantir que quando sair da maximização, se posicione no centro
                _windowSizeSettings.CentralizeWindow();
                _windowSizeSettings.MaximizeWindow();
            };
            InitializeComponent();
            
            if (string.IsNullOrWhiteSpace(filePath)) return;

            if (!File.Exists(filePath))
            {
                MessageBox.Show(
                    $"File not found:\n\n{filePath}",
                    "PDF not found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }
            
            AtualizarTextoZoom();            
        }

        private async Task LoadMenus() {
            Task[] arrTask = new Task[] { LoadMenuRecents(), LoadMenuPrefVisualQualitys(), LoadMenuLanguages() };
        }

        private async Task LoadMenuLanguages()
        {
            List<KeyValuePair< Enums.Langs, LanguageModel>> listLangs = LanguageSettings.GetLangs.OrderBy(x=>x.Key.ToString()).ToList();            
            await LoadPreferences();
            //Tenta reperar a configuração de linguagem de preferencias. Se não encontrar a configuração, seta inglês como linguagem default.
            string strLang = _preferences?.FirstOrDefault(x => x.Name == Enums.Preferences.Language)?.Value ?? 
                          listLangs.FirstOrDefault(x=> x.Key == Enums.Langs.English_US).Value.ToString()!;

            Enums.Langs lang = listLangs.FirstOrDefault(x => x.Value.ResourceFileIdentifier == strLang).Key;
            menuLanguage.Items.Clear();
            LanguageSettings.ChangeLanguage(lang);

            foreach (KeyValuePair<Enums.Langs, LanguageModel> itemLang in listLangs){
                MenuItem menuItem = new MenuItem
                {
                    Header = Application.Current.FindResource(itemLang.Value.ResourceValue),
                    Tag = itemLang
                };

                menuItem.Click += async (s, e) =>
                {
                    //Verifica se um outro idioma foi selecionado antes de recarregar os menus.
                    if (itemLang.Value.ResourceFileIdentifier != _preferences?.FirstOrDefault(x => x.Name == Enums.Preferences.Language)?.Value)
                    {
                        await _preferenceServices.Save(new Preference
                        {
                            Name = Enums.Preferences.Language,
                            Value = itemLang.Value.ResourceFileIdentifier
                        });

                        await Task.WhenAll(new Task[] { LoadMenuLanguages(), LoadMenuRecents() });
                    }
                };

                menuLanguage.Items.Add(menuItem);
            }

            CheckLanguage(lang);
        }

        private void CheckLanguage(Enums.Langs lang) {
            foreach (MenuItem menuItem in menuLanguage.Items) {
                if (menuItem.Header?.ToString()?.IndexOf("✔️") > -1 && ((KeyValuePair<Enums.Langs, LanguageModel>)menuItem.Tag).Key != lang)
                {
                    menuItem.Header = menuItem.Header.ToString()!.Replace("✔️", "");
                }
                else if (menuItem.Header?.ToString()?.IndexOf("✔️") == -1 && ((KeyValuePair<Enums.Langs, LanguageModel>)menuItem.Tag).Key == lang) {
                    menuItem.Header += " ✔️";
                }
            }
        }

        /// <summary>
        /// Monta os menus de qualidade visual, com base nas opções oferecidas pela biblioteca PDFium
        /// </summary>
        /// <param name="bitmapScalinhgMode">Indica qual valor deve ser aparecer com um check de selecionado.</param>
        /// <returns></returns>
        private async Task LoadMenuPrefVisualQualitys()
        {
            List<BitmapScalingMode> arrQualitiesValues = Enum.GetValues<BitmapScalingMode>().OrderBy(x => x).Distinct().ToList();
            List<string> arrQualitiesNames = Enum.GetNames<BitmapScalingMode>().OrderBy(x => x).ToList();

            foreach (var item in arrQualitiesValues) {
                //Existem opções do enum BitmapScalingMode que possuem o mesmo valor int. Por isso, junto essas opções numa só.
                string option = string.Join(" | ", arrQualitiesNames.Where(x => (int)Enum.Parse(typeof(BitmapScalingMode), x) == (int)item));

                MenuItem mi = new MenuItem
                {
                    Header = option
                };

                mi.Click += async delegate (object s, RoutedEventArgs e)
                {
                    BitmapScalingMode bitmapScalingMode = Enum.Parse<BitmapScalingMode>(option.Split('|')[0]);                    
                    await CheckQualityVisual(bitmapScalingMode);                    
                    await LoadPDF(_pdfInfo?.GetFullPath!);
                };

                menuVisualQuality.Items.Add(mi);
            }

            BitmapScalingMode bitmapScalingMode;
            bitmapScalingMode = Enum.Parse<BitmapScalingMode>(_preferences?.FirstOrDefault(x => x.Name == Enums.Preferences.VisualQuality)?.Value ?? BitmapScalingMode.NearestNeighbor.ToString());
            await CheckQualityVisual(bitmapScalingMode);
        }

        private async Task CheckQualityVisual(BitmapScalingMode bitmapScalingMode) {
            foreach (MenuItem item in menuVisualQuality.Items) {
                //Remove o check de um itemLang se ele era a seleção anterior e a seleção mudou.
                if (item.Header.ToString()!.IndexOf("✔️") > -1 && item.Header.ToString()!.IndexOf(bitmapScalingMode.ToString()) == -1)
                {
                    item.Header = item.Header.ToString()!.Replace("✔️", string.Empty);
                }
                else if (item.Header.ToString()!.IndexOf(bitmapScalingMode.ToString()) > -1) {
                    item.Header += " ✔️";
                }
            }
            RenderOptions.SetBitmapScalingMode(pdfViewer, bitmapScalingMode);
            await _preferenceServices.Save(new Preference { Name = Enums.Preferences.VisualQuality, Value = bitmapScalingMode.ToString() });
        }

        private async Task LoadMenuRecents()
        {
            ICollection<Recents> recents = await _recentServices.List();

            menuRecents.Items.Clear();

            foreach (Recents recent in recents) {
                string fileName = Path.GetFileName(recent.FileFullPath);
                Grid grid = new Grid() {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Height = 30,
                    Width = 300
                };
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star)});
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(35) });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(35) });

                TextBlock textBlock = new TextBlock()
                {
                    Text = StringUtils.AddReticence(fileName, 30, ".pdf"),
                    Margin = new Thickness(5),
                    Cursor = Cursors.Hand
                };
                Button btnFolder = new Button
                {
                    Content = "📂",
                    ToolTip = Application.Current.FindResource("OpenFolder"),
                    Tag = recent,
                    Background = System.Windows.Media.Brushes.Transparent,                    
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                };

                btnFolder.Click += (s, e) =>
                {
                    OpenFolderFromFilePath(s, e);
                    e.Handled = true;
                };

                Button btnRemove = new Button
                {
                    Content = "❌",
                    ToolTip = Application.Current.FindResource("RemoveFromRecents"),
                    Tag = recent,
                    Background = System.Windows.Media.Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand
                };
                btnRemove.Click += async (s, e) =>
                {
                    Button button = (Button)s;                   
                    await _recentServices.Delete(recent.Id);
                    await LoadMenuRecents();
                    e.Handled = true;
                };

                Grid.SetColumn(textBlock, 0);
                Grid.SetColumn(btnFolder, 1);
                Grid.SetColumn(btnRemove, 2);

                grid.Children.Add(textBlock);             
                grid.Children.Add(btnFolder);
                grid.Children.Add(btnRemove);                

                MenuItem item = new MenuItem
                {
                    Header = grid,
                    Style = (Style)FindResource("RecentMenuItemStyle"),
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    ToolTip = $"{recent.FileFullPath} - {recent.DthRegister.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture)}",
                    Tag = recent.FileFullPath
                };

                item.Click += async (s, e) => await LoadPDF(recent.FileFullPath);
                menuRecents.HorizontalContentAlignment = HorizontalAlignment.Right;
                menuRecents.Items.Add(item);

            }
        }

        public void OpenFolderFromFilePath(object sender, RoutedEventArgs e) {
            Button button = (Button)sender;
            Recents recent = (Recents)button.Tag;

            string? directory = Path.GetDirectoryName(recent.FileFullPath);
            if (!string.IsNullOrEmpty(directory)) {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = directory,
                    UseShellExecute = true
                });
            }
            e.Handled = true;
        }

        private async Task SaveInRecents() {
            if (_pdfInfo != null)
            {
                Recents recent = new Recents(_pdfInfo.GetFullPath);
                await _recentServices.Save(recent);
                await LoadMenuRecents();
            }
        }

        private async Task LoadPDF(string filePath) {
            //Se estiver tentando abrir o documento que já esta aberto, então ignora.
            if (_pdfInfo != null && _pdfInfo.GetFullPath == filePath) {
                return;
            }
            if (!string.IsNullOrEmpty(filePath))
            {
                if (!File.Exists(filePath)) {
                    if (!File.Exists(filePath))
                    {
                        MessageBox.Show(
                            $"File not found:\n\n{filePath}",
                            "PDF not found",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        return;
                    }
                }
                string fileName = Path.GetFileName(filePath);
                txtFileName.Text = $"📖 {StringUtils.AddReticence(fileName, 25, ".pdf")}";
                txtFileName.ToolTip = fileName;
                pdfViewer.Page = 1;
                pdfViewer.InvalidateVisual();
                pdfViewer.Document = PdfDocument.Load(filePath);
                pdfViewer.Zoom += _zoomInitial;

                _totalPages = pdfViewer.Document.PageCount;
                txtTotalPaginas.Text = _totalPages.ToString();

                Task[] listTask = new Task[2];

                CreateCurrentPdfInfo(filePath);
                Task[] arrTask = new Task[] { LoadLastPageReaded(), SaveInRecents() };
                await Task.WhenAll(arrTask);

                ResetZoom();
            }
        }

        private void CreateCurrentPdfInfo(string pdfPath) {
            string fileHash = FileServices.GetHashSHA256(pdfPath);

            _pdfInfo = new PdfInfo()
            {
                FilePath = Path.GetDirectoryName(pdfPath)!,
                FileName = Path.GetFileName(pdfPath),
                FileHash = fileHash
            };
        }

        private async Task LoadLastPageReaded()
        {
            var _pdfInfoResult = await _pdfInfoServices.FindPdfInfoByFullPath(_pdfInfo.FileHash);
            if (_pdfInfoResult != null)
            {
                _pdfInfo = _pdfInfoResult;
                //Delay para garantir que o pdf já foi carregado
                await Dispatcher.InvokeAsync(
                    async () =>
                    {
                        await Task.Delay(500);
                        SetActualPage(_pdfInfo.NumPage);                        
                    },
                    System.Windows.Threading.DispatcherPriority.Render
                );
            }
            else {
                await Dispatcher.InvokeAsync(() =>
                    SetActualPage(1),
                    System.Windows.Threading.DispatcherPriority.Render
                );
            }
        }

        private async void Abrir_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Open PDF",
                Filter = "PDF files (*.pdf)|*.pdf",
                DefaultExt = ".pdf",
                Multiselect = false
            };

            bool? resultDiaglog = dialog.ShowDialog();

            if (resultDiaglog != true) {
                return;
            }

            Task[] arrTask = new Task[] { LoadPDF(dialog.FileName), LoadMenuRecents() };
            await Task.WhenAll(arrTask);
        }

        private async Task LoadPreferences()
        {
            _preferences = await _preferenceServices.List();
        }

        private async void PdfViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // A rolagem mudou
            if(pdfViewer.Page != (_currentPage - 1))
            {
                _currentPage = pdfViewer.Page + 1;
                if (_pdfInfo != null)
                {
                    _pdfInfo.NumPage = _currentPage;
                    if (_pdfInfo.NumPage == 1)
                    {
                        await _pdfInfoServices.deleteAsync(_pdfInfo.Id);
                    }
                    else
                    {
                        await _pdfInfoServices.Save(_pdfInfo);
                    }
                    SetActualPage(pdfViewer.Page + 1);
                }
            }
        }

        private void Anterior_Click(object sender, RoutedEventArgs e)
        {
            int currentPage = Math.Max(pdfViewer.Page - 1, 0);
            pdfViewer.Page = currentPage;
            txtPagina.Text = (currentPage + 1).ToString();
            _currentPage = currentPage + 1;
        }

        private void Proxima_Click(object sender, RoutedEventArgs e)
        {
            int currentPage = Math.Min(pdfViewer.Page + 1, _totalPages);
            pdfViewer.Page = currentPage;
            txtPagina.Text = (currentPage + 1).ToString();
            _currentPage = currentPage + 1;
        }

        private void ZoomMenos_Click(object sender, RoutedEventArgs e)
        {
            ZoomMenos();
        }

        private void ZoomMenos()
        {
            if (pdfViewer.Zoom > 1.0)
            {
                pdfViewer.Zoom = Math.Max(0.25, pdfViewer.Zoom - 0.25);
                CentralizarHorizontalmente();
                AtualizarTextoZoom();
            }
        }

        private void ResetZoom() {
            pdfViewer.Zoom = 1 + _zoomInitial;
            AtualizarTextoZoom();
        }

        private void ZoomMais_Click(object sender, RoutedEventArgs e)
        {
            ZoomMais();
        }

        private void ZoomMais() {
            if (pdfViewer.Zoom < 3.50)
            {
                pdfViewer.Zoom = Math.Min(5.0, pdfViewer.Zoom + 0.25);
                CentralizarHorizontalmente();
                AtualizarTextoZoom();
            }
        }

        private void AtualizarTextoZoom()
        {
            txtZoom.Text = $"{(pdfViewer.Zoom - _zoomInitial) * 100:0}%";
        }

        private void PdfViewr_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            if (Keyboard.Modifiers != ModifierKeys.Control) {
                return;
            }

            e.Handled = true;

            if (e.Delta > 0)
            {
                ZoomMais();
            }
            else {
                ZoomMenos();
            }
        }

        private void txtPagina_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (int.TryParse(txtPagina.Text, out int value))
                {
                    if (value < 1) value = 1;
                    SetActualPage(value);
                }
            }
        }

        private void SetActualPage(int page) {
            if (pdfViewer != null)
            {
                if (page < 1) page = 1;
                pdfViewer.Page = Math.Min(page - 1, _totalPages);
                txtPagina.Text = (pdfViewer.Page + 1).ToString();
                _pdfInfo.NumPage = pdfViewer.Page + 1;
            }
        }

        private static T? FindVisualChild<T>(DependencyObject parent)
            where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T result)
                    return result;

                var descendant = FindVisualChild<T>(child);

                if (descendant != null)
                    return descendant;
            }

            return null;
        }

        private void CentralizarHorizontalmente()
        {
            var scrollViewer = FindVisualChild<ScrollViewer>(pdfViewer);

            if (scrollViewer == null) return;

            double offset = (scrollViewer.ExtentWidth - scrollViewer.ViewportWidth) / 2;

            scrollViewer.ScrollToHorizontalOffset(offset);
        }

        private void window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.Add || e.Key == Key.OemPlus)
                {
                    ZoomMais_Click(null, null);
                }
                else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
                {
                    ZoomMenos_Click(null, null);
                }
                else if (e.Key == Key.D0 || e.Key == Key.NumPad0) {
                    ResetZoom();
                }
            }
        }

        private void window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_spacePressing) return;

            if (e.Key == Key.Space)
            {
                _spacePressing = true;
                pdfViewer.Cursor = Cursors.Hand;
                e.Handled = true;
            }
        }

        private void pdfViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_spacePressing) return;

            _mouseDragging = true;
            _lastPositionMouse = e.GetPosition(pdfViewer);
            pdfViewer.CaptureMouse();
            pdfViewer.Cursor = Cursors.SizeAll;
            e.Handled = true;
        }

        private void pdfViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_mouseDragging) return;

            _mouseDragging = false;
            pdfViewer.ReleaseMouseCapture();

            pdfViewer.Cursor = _spacePressing ? Cursors.Hand : Cursors.Arrow;

            e.Handled = true;
        }

        private void pdfViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_mouseDragging) return;

            var scrollViewer = FindVisualChild<ScrollViewer>(pdfViewer);
            if (scrollViewer == null) return;

            System.Windows.Point actualPosition = e.GetPosition(pdfViewer);

            double deltaX = actualPosition.X - _lastPositionMouse.X;
            double deltaY = actualPosition.Y - _lastPositionMouse.Y;

            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - deltaX);
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - deltaY);

            _lastPositionMouse = actualPosition;

            e.Handled = true;
        }

        private void window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space) {
                _spacePressing = false;
                if (!_mouseDragging) { 
                    pdfViewer.Cursor = Cursors.Arrow;
                }

                e.Handled = true;
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            string? selectedText = pdfViewer.Document.GetSelectionText();
            if (string.IsNullOrEmpty(selectedText)) return;

            Clipboard.SetText(selectedText);


        }

        private static void RemoveConfigRegisterWindows()
        {
            string exeName = System.IO.Path.GetFileName(Environment.ProcessPath)!;

            Registry.CurrentUser.DeleteSubKeyTree(
                $@"Software\Classes\Applications\{exeName}",
                false);

            Registry.CurrentUser.DeleteSubKeyTree(
                $@"Software\Classes\.pdf\OpenWithList\{exeName}",
                false);
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length == 1 && Path.GetExtension(files[0]).Equals(".pdf", StringComparison.OrdinalIgnoreCase)) {
                    e.Effects = DragDropEffects.Copy;
                    e.Handled = true;
                    return;
                }
            }
        }

        private async void Window_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length != 1)
            {
                return;
            }

            string pathPdfFile = files[0];

            if (!Path.GetExtension(pathPdfFile).Equals(".pdf", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            await LoadPDF(pathPdfFile);
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ButtonMaxmize_Click(object sender, RoutedEventArgs e)
        {
            _windowSizeSettings.MaximizeWindow();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ClickCount >= 2) {
                    _windowSizeSettings.MaximizeWindow();
                    return;
                }
                if (e.LeftButton != MouseButtonState.Pressed)
                    return;

                if (_windowSizeSettings.IsMaximized)
                    return;
                this.Cursor = Cursors.SizeAll;
                DragMove();
            }
            finally {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow()
            {
                Owner = this
            };
            aboutWindow.ShowDialog();
        }
    }
}