using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;

namespace PDFViewLastReadedPage.Configuration
{
    internal class WindowSizeSettings
    {
        private const double MAXIMIZE_COMPENSATION = 10;
        private double _normalLeft;
        private double _normalTop;
        private double _normalWidth;
        private double _normalHeight;
        private Window _window;
        public bool IsMaximized { get; private set; }        

        public WindowSizeSettings(Window window)
        {
            _window = window;
        }

        internal void MaximizeWindow()
        {
            var workArea = SystemParameters.WorkArea;
            if (!IsMaximized)
            {
                // Guarda o estado atual
                _normalTop = _window.Top;
                _normalLeft = _window.Left;
                _normalWidth = _window.Width;
                _normalHeight = _window.Height;
                // Área disponível da tela, sem a barra de tarefas                
                _window.Left = workArea.Left - MAXIMIZE_COMPENSATION;
                _window.Top = workArea.Top - MAXIMIZE_COMPENSATION;
                _window.Width = workArea.Width + (MAXIMIZE_COMPENSATION * 2);
                _window.Height = workArea.Height + (MAXIMIZE_COMPENSATION * 2);

                IsMaximized = true;
            }
            else {
                _window.Width =  _normalWidth;
                _window.Height = _normalHeight;
                //_window.Left = workArea.Left + (workArea.Width - _window.Width) / 2;
                //_window.Top = (workArea.Top + (workArea.Height - _window.Height) / 2) - 10;
                _window.Left = _normalLeft;
                _window.Top = _normalTop;

                IsMaximized = false;
            }
        }

        internal void CentralizeWindow() {
            var workArea = SystemParameters.WorkArea;
            _window.Left = workArea.Left + (workArea.Width - _window.Width) / 2;
            _window.Top = (workArea.Top + (workArea.Height - _window.Height) / 2) - 10;            
        }
    }

}
