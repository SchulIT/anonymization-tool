using AnonymizationTool.Data.Persistence;
using AnonymizationTool.Messages;
using AnonymizationTool.ViewModels;
using Fluent;
using KPreisser.UI;
using Microsoft.Win32;
using NaturalSort.Extension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Interop;

namespace AnonymizationTool.Views
{
    /// <summary>
    /// Interaktionslogik für MainView.xaml
    /// </summary>
    public partial class MainView : RibbonWindow
    {
        public MainView()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var cvs = Resources["GroupedStudents"] as CollectionViewSource;
            if (cvs.View != null)
            {
                var view = cvs.View as ListCollectionView;

                if (view != null)
                {
                    view.CustomSort = new StudentComparer();
                }
            }

            var locator = App.Current.Resources["ViewModelLocator"] as ViewModelLocator;
            
            var messenger = locator.Messenger;
            messenger.Register<DialogMessage>(this, OnDialogMessage);
            messenger.Register<ErrorDialogMessage>(this, OnErrorDialogMessage);
            messenger.Register<SelectDirectoryDialogMessage>(this, OnSelectDirectoryDialogMessage);

            locator.Main.LoadStudents();
        }

        private void OnDialogMessage(DialogMessage msg)
        {
            var page = new TaskDialogPage
            {
                Title = msg.Title,
                Text = msg.Text,
                Instruction = msg.Header,
                Icon = TaskDialogStandardIcon.Information
            };

            var dialog = new TaskDialog(page);
            dialog.Show(new WindowInteropHelper(this).Handle);
        }

        private void OnErrorDialogMessage(ErrorDialogMessage msg)
        {
            var page = new TaskDialogPage
            { 
                Title = msg.Title,
                Text = msg.Text,
                Instruction = msg.Header,
                Icon = TaskDialogStandardIcon.Error,
                Expander =
                {
                    Text = msg.Exception.Message,
                    ExpandFooterArea = true
                }
            };

            var dialog = new TaskDialog(page);
            dialog.Show(new WindowInteropHelper(this).Handle);
        }

        private void OnSelectDirectoryDialogMessage(SelectDirectoryDialogMessage msg)
        {
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();

            msg.Path = dialog.SelectedPath;
        }

        private class StudentComparer : IComparer
        {
            private static readonly IComparer<string> stringComparer = StringComparer.Create(new System.Globalization.CultureInfo("de"), false).WithNaturalSort();

            public int Compare(object x, object y)
            {
                var studentX = x as AnonymousStudent;
                var studentY = y as AnonymousStudent;

                if(studentX == null || studentY == null)
                {
                    return -1; // This should not happen (actually)
                }

                // Sort by grade
                var gradeCmp = stringComparer.Compare(studentX.Grade, studentY.Grade);
                if (gradeCmp != 0)
                {
                    return gradeCmp;
                }

                // Sort by lastname
                var lastNameCmp = stringComparer.Compare(studentX.LastName, studentY.LastName);
                if (lastNameCmp != 0)
                {
                    return lastNameCmp;
                }

                // Sort by firstname
                return stringComparer.Compare(studentX.FirstName, studentY.FirstName);
            }
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnRequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
