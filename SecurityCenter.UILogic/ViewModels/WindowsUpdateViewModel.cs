﻿using SecurityCenter.Business.Models;
using SecurityCenter.UILogic.ViewModels.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityCenter.UILogic.ViewModels
{
    public class WindowsUpdateViewModel : ViewModel<WindowsUpdate>
    {
        public WindowsUpdateViewModel(WindowsUpdate model) : base(model)
        {
        }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }

        public string Name
        {
            get => Model.Name;
        }

        public string Description
        {
            get => Model.Description;
        }

        public string KbNumber
        {
            get => Model.KbNumber;
        }
    }
}