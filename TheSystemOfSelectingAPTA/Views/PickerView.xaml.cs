using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using TheSystemOfSelectingAPTA.Models;

namespace TheSystemOfSelectingAPTA.Views
{
    public partial class PickerView : ContentPage, INotifyPropertyChanged
    {
        private readonly PickerModel _model = new PickerModel();
        private readonly Random _randomGenerator = new Random();

        public ObservableCollection<PickerModel.PickerStudent> Students { get; } = new ObservableCollection<PickerModel.PickerStudent>();
        public ObservableCollection<PickerModel.PickerClass> Classes { get; } = new ObservableCollection<PickerModel.PickerClass>();
        public ObservableCollection<string> LuckyResults { get; } = new ObservableCollection<string>();

        private PickerModel.PickerClass? _selectedClass;
        public PickerModel.PickerClass? SelectedClass
        {
            get { return _selectedClass; }
            set
            {
                if (_selectedClass == value)
                {
                    return;
                }
                _selectedClass = value;
                OnPropertyChanged();
                LoadSelectedClass();
            }
        }

        private string _newStudentName = string.Empty;
        public string NewStudentName
        {
            get { return _newStudentName; }
            set
            {
                if (_newStudentName == value)
                {
                    return;
                }
                _newStudentName = value;
                OnPropertyChanged();
            }
        }

        private string _newClassName = string.Empty;
        public string NewClassName
        {
            get { return _newClassName; }
            set
            {
                if (_newClassName == value)
                {
                    return;
                }
                _newClassName = value;
                OnPropertyChanged();
            }
        }

        private string _result = string.Empty;
        public string Result
        {
            get { return _result; }
            set
            {
                if (_result == value)
                {
                    return;
                }
                _result = value;
                OnPropertyChanged();
            }
        }

        public PickerView()
        {
            InitializeComponent();
            BindingContext = this;
            _model.ClearAllLuckyFlags();
            LoadClassesFromModel();
        }

        private void LoadClassesFromModel()
        {
            Classes.Clear();

            List<string> classNames = _model.GetClassNames();

            foreach (string className in classNames)
            {
                List<PickerModel.PickerStudent> loadedStudents = _model.LoadClass(className);

                PickerModel.PickerClass pickerClass = new PickerModel.PickerClass
                {
                    Name = className,
                    Students = loadedStudents
                };

                Classes.Add(pickerClass);
            }

            if (Classes.Count > 0)
            {
                SelectedClass = Classes[0];
            }
        }

        private void LoadSelectedClass()
        {
            Students.Clear();

            if (SelectedClass == null)
            {
                return;
            }

            foreach (PickerModel.PickerStudent student in SelectedClass.Students)
            {
                Students.Add(student);
            }
        }

        private void OnAddClicked(object sender, EventArgs eventArgs)
        {
            if (string.IsNullOrWhiteSpace(NewStudentName) || SelectedClass == null)
            {
                return;
            }

            int nextId = _model.GetNextIdForClass(SelectedClass.Name);

            PickerModel.PickerStudent newStudent = new PickerModel.PickerStudent
            {
                ClassName = SelectedClass.Name,
                Id = nextId,
                Name = NewStudentName.Trim(),
                IsPresent = true,
                HasLuckyNumber = false
            };

            SelectedClass.Students.Add(newStudent);
            Students.Add(newStudent);
            _model.SaveClass(SelectedClass.Name, SelectedClass.Students);
            NewStudentName = string.Empty;
        }

        private void OnSaveClicked(object sender, EventArgs eventArgs)
        {
            if (SelectedClass == null)
            {
                return;
            }

            _model.SaveClass(SelectedClass.Name, SelectedClass.Students);
        }

        private void OnDeleteClicked(object sender, EventArgs eventArgs)
        {
            if (SelectedClass == null)
            {
                return;
            }

            _model.DeleteClass(SelectedClass.Name);
            LoadClassesFromModel();
        }

        private void OnNewClassClicked(object sender, EventArgs eventArgs)
        {
            if (string.IsNullOrWhiteSpace(NewClassName))
            {
                return;
            }

            string trimmedClassName = NewClassName.Trim();

            _model.CreateClass(trimmedClassName);
            LoadClassesFromModel();

            PickerModel.PickerClass? foundClass = null;
            foreach (PickerModel.PickerClass pickerClass in Classes)
            {
                if (pickerClass.Name == trimmedClassName)
                {
                    foundClass = pickerClass;
                    break;
                }
            }

            SelectedClass = foundClass;
            NewClassName = string.Empty;
        }

        private void OnDrawClicked(object sender, EventArgs eventArgs)
        {
            if (SelectedClass == null)
            {
                return;
            }

            List<PickerModel.PickerStudent> eligibleStudents = new List<PickerModel.PickerStudent>();
            foreach (PickerModel.PickerStudent student in SelectedClass.Students)
            {
                if (student.IsPresent && !student.HasLuckyNumber)
                {
                    eligibleStudents.Add(student);
                }
            }

            if (eligibleStudents.Count > 0)
            {
                Result = eligibleStudents[_randomGenerator.Next(eligibleStudents.Count)].Name;
                return;
            }

            List<PickerModel.PickerStudent> presentStudents = new List<PickerModel.PickerStudent>();
            foreach (PickerModel.PickerStudent student in SelectedClass.Students)
            {
                if (student.IsPresent)
                {
                    presentStudents.Add(student);
                }
            }

            if (presentStudents.Count == 0)
            {
                Result = "Brak dostępnych uczniów";
                return;
            }

            Result = presentStudents[_randomGenerator.Next(presentStudents.Count)].Name;
        }

        private void OnAssignLuckyClicked(object sender, EventArgs eventArgs)
        {
            LuckyResults.Clear();

            _model.ClearAllLuckyFlags();

            int maxId = 0;
            foreach (PickerModel.PickerClass pickerClass in Classes)
            {
                foreach (PickerModel.PickerStudent student in pickerClass.Students)
                {
                    if (student.Id > maxId)
                    {
                        maxId = student.Id;
                    }
                }
            }

            if (maxId == 0)
            {
                LuckyResults.Add("Brak uczniów we wszystkich klasach");
                return;
            }

            int pickedId = _randomGenerator.Next(1, maxId + 1);

            _model.AssignLuckyNumber(pickedId);

            LoadClassesFromModel();

            LuckyResults.Add($"Szczęśliwy numerek: #{pickedId}");
        }

        public new event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}