using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.ViewModel.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MusicPlayerForDrummers.ViewModel
{
    public class MainVM : BaseViewModel
    {
        private LibraryVM LibraryVM { get; set; }
        private  PartitionVM PartitionVM { get; set; }
        private SyncVM SyncVM { get; set; }

        public List<BaseViewModel> ViewModels;

        private BaseViewModel _currentViewModel;
        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set => SetField(ref _currentViewModel, value); 
        }

        private string _leftViewTitle;
        public string LeftViewTitle
        {
            get => _leftViewTitle;
            set => SetField(ref _leftViewTitle, value);
        }
        private string _rightViewTitle;
        public string RightViewTitle
        {
            get => _rightViewTitle;
            set => SetField(ref _rightViewTitle, value);
        }
        private bool _canSwitchViewRight;
        public bool CanSwitchViewRight
        {
            get => _canSwitchViewRight;
            set
            {
                SetField(ref _canSwitchViewRight, value);
                SwitchViewRightCommand.RaiseCanExecuteChanged(); //TODO: Performance? is necessary? (since the button shouldnt appear anyway)
            }

        }
    private bool _canSwitchViewLeft;
        public bool CanSwitchViewLeft
        {
            get => _canSwitchViewLeft;
            set
            {
                SetField(ref _canSwitchViewLeft, value);
                SwitchViewLeftCommand.RaiseCanExecuteChanged();
            }
        }
        public override string ViewModelName => "MAIN";

        public MainVM()
        {
            DBHandler.InitializeDatabase();

            SwitchViewLeftCommand = new DelegateCommand(x => SwitchView(EDirection.Left), x => CanSwitchViewLeft);
            SwitchViewRightCommand = new DelegateCommand(x => SwitchView(EDirection.Right), x => CanSwitchViewRight);

            LibraryVM = new LibraryVM();
            PartitionVM = new PartitionVM();
            ViewModels = new List<BaseViewModel> { LibraryVM, PartitionVM };
            SetView(LibraryVM);
        }

        public DelegateCommand SwitchViewLeftCommand { get; private set; }
        public DelegateCommand SwitchViewRightCommand { get; private set; }
        
        private void SetView(BaseViewModel view)
        {
            if(CurrentViewModel == view)
                return;
            
            CurrentViewModel = view;
            int index = ViewModels.IndexOf(view);
            if(index > 0)
            {
                LeftViewTitle = ViewModels[index - 1].ViewModelName;
                CanSwitchViewLeft = true;
            }
            else
            {
                CanSwitchViewLeft = false;
            }

            if (index < ViewModels.Count - 1)
            {
                RightViewTitle = ViewModels[index + 1].ViewModelName;
                CanSwitchViewRight = true;
            }
            else
            {
                CanSwitchViewRight = false;
            }
        }

        private void SwitchView(EDirection direction)
        {
            int currentIndex = ViewModels.IndexOf(CurrentViewModel);
            int newIndex = currentIndex + (int)direction;

            if(newIndex >= 0 && newIndex <= (ViewModels.Count - 1))
            {
                SetView(ViewModels[newIndex]);
            }
            else
            {
                string warning = "Invalid switch view from '" + CurrentViewModel.ViewModelName + "' to the ";
                warning += direction == EDirection.Left ? "Left" : "Right";
                Trace.WriteLine(warning);
            }
        }

        public enum EDirection
        {
            Left = -1,
            Right = +1
        }
    }
}
