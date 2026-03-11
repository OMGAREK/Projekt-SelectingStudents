using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TheSystemOfSelectingAPTA.Models
{
    public class PickerModel
    {
        private readonly string dataFilePath = Path.Combine(FileSystem.AppDataDirectory, "students.txt");
        private readonly string classesFilePath = Path.Combine(FileSystem.AppDataDirectory, "classes.txt");

        public PickerModel()
        {
            if (!File.Exists(dataFilePath))
            {
                File.WriteAllText(dataFilePath, string.Empty);
            }

            if (!File.Exists(classesFilePath))
            {
                File.WriteAllText(classesFilePath, string.Empty);
            }
        }

        public List<string> GetClassNames()
        {
            List<string> classNames = new List<string>();

            string[] classLines = File.ReadAllLines(classesFilePath);
            foreach (string line in classLines)
            {
                string trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed) && !classNames.Contains(trimmed))
                {
                    classNames.Add(trimmed);
                }
            }

            List<PickerStudent> allStudents = LoadAllStudents();
            foreach (PickerStudent student in allStudents)
            {
                if (!classNames.Contains(student.ClassName))
                {
                    classNames.Add(student.ClassName);
                }
            }

            return classNames;
        }

        public void CreateClass(string className)
        {
            List<string> classNames = GetClassNames();

            if (classNames.Contains(className))
            {
                return;
            }

            classNames.Add(className);
            WriteClassNames(classNames);
        }

        public void DeleteClass(string className)
        {
            List<string> classNames = GetClassNames();
            List<string> remaining = new List<string>();
            foreach (string name in classNames)
            {
                if (name != className)
                {
                    remaining.Add(name);
                }
            }
            WriteClassNames(remaining);

            List<PickerStudent> allStudents = LoadAllStudents();
            List<PickerStudent> otherStudents = new List<PickerStudent>();
            foreach (PickerStudent student in allStudents)
            {
                if (student.ClassName != className)
                {
                    otherStudents.Add(student);
                }
            }
            WriteAllStudents(otherStudents);
        }

        public List<PickerStudent> LoadAllStudents()
        {
            List<PickerStudent> allStudents = new List<PickerStudent>();

            string[] lines = File.ReadAllLines(dataFilePath);
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                PickerStudent student = ParseLine(line);
                if (student != null)
                {
                    allStudents.Add(student);
                }
            }

            return allStudents;
        }

        public List<PickerStudent> LoadClass(string className)
        {
            List<PickerStudent> allStudents = LoadAllStudents();
            List<PickerStudent> classStudents = new List<PickerStudent>();

            foreach (PickerStudent student in allStudents)
            {
                if (student.ClassName == className)
                {
                    classStudents.Add(student);
                }
            }

            return classStudents;
        }

        public void SaveClass(string className, IEnumerable<PickerStudent> updatedStudents)
        {
            CreateClass(className);

            List<PickerStudent> allStudents = LoadAllStudents();

            List<PickerStudent> otherStudents = new List<PickerStudent>();
            foreach (PickerStudent student in allStudents)
            {
                if (student.ClassName != className)
                {
                    otherStudents.Add(student);
                }
            }

            foreach (PickerStudent student in updatedStudents)
            {
                student.ClassName = className;
                otherStudents.Add(student);
            }

            WriteAllStudents(otherStudents);
        }

        public void ClearAllLuckyFlags()
        {
            List<PickerStudent> allStudents = LoadAllStudents();

            foreach (PickerStudent student in allStudents)
            {
                student.HasLuckyNumber = false;
            }

            WriteAllStudents(allStudents);
        }

        public void AssignLuckyNumber(int luckyId)
        {
            List<PickerStudent> allStudents = LoadAllStudents();

            foreach (PickerStudent student in allStudents)
            {
                student.HasLuckyNumber = (student.Id == luckyId);
            }

            WriteAllStudents(allStudents);
        }

        public int GetNextIdForClass(string className)
        {
            List<PickerStudent> classStudents = LoadClass(className);

            int maxId = 0;
            foreach (PickerStudent student in classStudents)
            {
                if (student.Id > maxId)
                {
                    maxId = student.Id;
                }
            }

            return maxId + 1;
        }

        private PickerStudent ParseLine(string line)
        {
            string[] parts = line.Split('|');

            if (parts.Length < 5)
            {
                return null;
            }

            string className = parts[0].Trim();
            string name = parts[2].Trim();

            if (string.IsNullOrWhiteSpace(className) || string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            if (!int.TryParse(parts[1].Trim(), out int id))
            {
                return null;
            }

            bool.TryParse(parts[3].Trim(), out bool isPresent);
            bool.TryParse(parts[4].Trim(), out bool hasLuckyNumber);

            return new PickerStudent
            {
                ClassName = className,
                Id = id,
                Name = name,
                IsPresent = isPresent,
                HasLuckyNumber = hasLuckyNumber
            };
        }

        private void WriteAllStudents(List<PickerStudent> students)
        {
            List<string> lines = new List<string>();

            foreach (PickerStudent student in students)
            {
                lines.Add($"{student.ClassName}|{student.Id}|{student.Name}|{student.IsPresent}|{student.HasLuckyNumber}");
            }

            File.WriteAllLines(dataFilePath, lines);
        }

        private void WriteClassNames(List<string> classNames)
        {
            File.WriteAllLines(classesFilePath, classNames);
        }

        public class PickerClass
        {
            public string Name { get; set; } = string.Empty;
            public List<PickerStudent> Students { get; set; } = new List<PickerStudent>();
        }

        public class PickerStudent
        {
            public string ClassName { get; set; } = string.Empty;
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public bool IsPresent { get; set; } = true;
            public bool HasLuckyNumber { get; set; } = false;

            public override string ToString()
            {
                return Name;
            }
        }
    }
}