using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;

namespace TaskPlanner
{
    public partial class MainPage : ContentPage
    {
        private readonly HttpClient _httpClient = new();
        private const string FirebaseUrl = "https://test-8c2d2-default-rtdb.firebaseio.com/tasks.json";


        public ObservableCollection<TaskItem> Tasks { get; set; } = new();

        public MainPage()
        {
            InitializeComponent();
            TaskListView.ItemsSource = Tasks;
            LoadTasksFromCloud();
        }

        public class TaskItem
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Priority { get; set; }
            public DateTime DueDate { get; set; }
        }

        private async void LoadTasksFromCloud()
        {
            try
            {
                var response = await _httpClient.GetStringAsync(FirebaseUrl);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    var tasks = JsonConvert.DeserializeObject<Dictionary<string, TaskItem>>(response);
                    if (tasks != null)
                        foreach (var task in tasks.Values)
                            Tasks.Add(task);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load tasks: {ex.Message}", "OK");
            }
        }

        private async void OnAddTaskClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskNameEntry.Text) || PriorityPicker.SelectedItem == null)
            {
                await DisplayAlert("Error", "Please fill in all fields.", "OK");
                return;
            }

            var newTask = new TaskItem
            {
                Name = TaskNameEntry.Text,
                Description = TaskDescriptionEditor.Text,
                Priority = PriorityPicker.SelectedItem.ToString(),
                DueDate = DueDatePicker.Date
            };

            Tasks.Add(newTask);

            var json = JsonConvert.SerializeObject(newTask);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                await _httpClient.PostAsync(FirebaseUrl, content);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to save task: {ex.Message}", "OK");
            }
        }
    }
}
