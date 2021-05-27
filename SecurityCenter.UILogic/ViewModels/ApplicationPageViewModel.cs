﻿using SecurityCenter.Data.System;
using SecurityCenter.UILogic.ViewModels.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using SecurityCenter.UILogic.Commands;
using System.Windows.Input;
using System.Threading.Tasks;

namespace SecurityCenter.UILogic.ViewModels
{
    public class ApplicationPageViewModel : ViewModelBase
    {

        public ApplicationPageViewModel()
        {
            // Initialize all the commands.
            UninstallApplicationCommand = new RelayCommand(UninstallApplication);
            RefreshApplicationsCommand = new RelayCommand(RefreshApplications);

            // Get all applications of this system and store them into the ViewModel.
            Applications = new ApplicationCollectionViewModel(SystemAccess.GetApplications());
            FilteredApplications = Applications.AsEnumerable();
        }

        public ICommand RefreshApplicationsCommand { get; private set; }
        private void RefreshApplications(object obj)
        {
            if (!IsRefreshingApplications)
            {
                IsRefreshingApplications = true;

                Task.Run(() =>
                {
                    Applications = new ApplicationCollectionViewModel(SystemAccess.GetApplications());
                    FilteredApplications = Applications.AsEnumerable();
                    FilterText = "";

                    IsRefreshingApplications = false;
                });
            }
        }

        public ICommand UninstallApplicationCommand { get; private set; }
        private void UninstallApplication(object obj)
        {
            string uninstallPath = obj as string;
            SystemAccess.UninstallApplication(uninstallPath);
        }

        private ApplicationCollectionViewModel applications;
        public ApplicationCollectionViewModel Applications
        {
            get => applications;
            set => SetProperty(ref applications, value);
        }

        private IEnumerable<ApplicationViewModel> filteredApplications;
        public IEnumerable<ApplicationViewModel> FilteredApplications
        {
            get => filteredApplications;
            set => SetProperty(ref filteredApplications, value);
        }

        private bool isRefreshingApplications = false;
        public bool IsRefreshingApplications
        {
            get => isRefreshingApplications;
            set => SetProperty(ref isRefreshingApplications, value);
        }

        private string filterText;
        public string FilterText
        {
            get => filterText;
            set
            {
                SetProperty(ref filterText, value);
                FilteredApplications = Applications.Where(a => Regex.IsMatch(a.Name.ToLowerInvariant(), filterText.ToLowerInvariant()));
            }
        }
    }
}
