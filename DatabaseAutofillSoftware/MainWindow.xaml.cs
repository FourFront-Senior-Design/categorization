﻿using System;
using ViewModelInterfaces;
using System.Diagnostics;
using System.Windows;
using ServicesInterface;

namespace DatabaseAutofillSoftware
{
    public partial class MainWindow : Window
    {
        IMainWindowVM _viewModel;
        IOCRService _ocrService;
        IAutofillController _autofillService;

        public MainWindow(IMainWindowVM viewModel, IOCRService GoogleVision, IAutofillController autofillController)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            _ocrService = GoogleVision;
            _autofillService = autofillController;
        }
        
        private void BrowseClick(object sender, RoutedEventArgs e)
        {
            _viewModel.Message = "";
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.ShowDialog();

            string selectedPath = dialog.SelectedPath;
            if (selectedPath != string.Empty)
            {
                _viewModel.FileLocation = selectedPath;
            }
        }

        private void OCRClick(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Printing...");
            Trace.WriteLine(_viewModel.FileLocation);

            int countData = _viewModel.LoadData();

            if (countData == -1)
            {
                _viewModel.Message = "Invalid Path. Try Again.";
            }
            else if (countData == 0)
            {
                _viewModel.Message = "No database found. Try Again.";
            }
            else
            {
                Properties.Settings.Default.databaseFilePath = _viewModel.FileLocation;
                Properties.Settings.Default.Save();

                _ocrService.extractText(_viewModel.FileLocation);

                _viewModel.Message = "Successfully processed " + countData.ToString() +
                                 " records.";
                _viewModel.EnableRun = true;
            }
        }

        private void AutofillClick(object sender, RoutedEventArgs e)
        {
            _autofillService.runScripts(_viewModel.FileLocation);

            // call ms access interface and push data to database
        }

        private void OnTextChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.EnableRun = false;
            _viewModel.Message = "";
        }

        private void ExitClick(object sender, RoutedEventArgs e)
        {
            this.Close();
            Application.Current.Shutdown();
        }
    }
}
