using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Diplom.Models;
using System.Windows;
using static System.Net.WebRequestMethods;
using System.Linq;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5254/api/") // Замените на ваш адрес API
        };
    }

    public async Task<bool> CheckApiConnectionAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("users");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<User>> GetUsersAsync()
    {
        var response = await _httpClient.GetAsync("users");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<User>>(json);
    }

    public async Task<User> GetUserAsync(int id)
    {
        var response = await _httpClient.GetAsync($"users/{id}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<User>(json);
    }

    public async Task<User> GetUserAsync(string login)
    {
        try
        {
            var response = await _httpClient.GetAsync($"users/login/{login}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<User>(json);
        }
        catch
        {
            return null;
        }
    }

    public async Task<User> CreateUserAsync(User user)
    {
        var json = JsonConvert.SerializeObject(user);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("users", content);
        response.EnsureSuccessStatusCode();

        json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<User>(json);
    }


    public async Task UpdateUserAsync(int id, User user)
    {
        var json = JsonConvert.SerializeObject(user);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync($"users/{id}", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteUserAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"users/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task PostFile(File file)
    {
        string base64FileData = Convert.ToBase64String(file.FileData);

        // Подготовка объекта для сериализации
        var fileToSend = new
        {
            fileName = file.FileName,
            filePath = file.FilePath,
            iduser = file.Iduser,
            fileData = base64FileData,
            creationDate = file.CreationDate.ToString("o") // Форматирование даты в ISO 8601
        };

        var json = JsonConvert.SerializeObject(fileToSend);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("Files", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IEnumerable<FileImport>> GetFilesByUserIdAsync(int userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"files/user/{userId}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var files = await response.Content.ReadFromJsonAsync<IEnumerable<FileImport>>();
                return files.OrderByDescending(f => f.CreationDate);
            }
            else
            {
                throw new Exception($"Error fetching files: {response.ReasonPhrase}");
            }
        }
        catch(Exception ex) 
        {
            return null;
        }
        
    }

    public async Task<IEnumerable<FileImport>> GetFiles()
    {
        try
        {
            var response = await _httpClient.GetAsync($"files/");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var files = await response.Content.ReadFromJsonAsync<IEnumerable<FileImport>>();
                return files.OrderByDescending(f => f.CreationDate);
            }
            else
            {
                throw new Exception($"Error fetching files: {response.ReasonPhrase}");
            }
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public async Task DeleteFileAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"files/{id}");
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException httpEx)
        {
            MessageBox.Show(httpEx.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (TaskCanceledException timeoutEx)
        {
            MessageBox.Show(timeoutEx.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

