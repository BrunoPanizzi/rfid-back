using System.Net;
using System.Net.Http.Json;
using tests.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

using Backend.Features.Users;
using Backend.Database;

namespace tests.Features.Users;

public class UserControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public async Task InitializeAsync()
    {
        await factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetUsers_ShouldReturnSuccess()
    {
        var response = await _client.GetAsync("/api/users");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var users = await response.Content.ReadFromJsonAsync<List<User>>();
        Assert.NotNull(users);
        Assert.Empty(users);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnCreatedUser()
    {
        var newUser = new CreateUserDto { Name = "Fulaninho", Email = "fulano@email.com" };

        var response = await _client.PostAsync("/api/users", JsonContent.Create(newUser));

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdUser = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(createdUser);
        Assert.Equal(newUser.Name, createdUser.Name);
        Assert.Equal(newUser.Email, createdUser.Email);
    }

    [Fact]
    public async Task GetUser_ShouldReturnCreatedUser()
    {
        var newUser = new CreateUserDto { Name = "Fulaninho", Email = "fulano@email.com" };

        var createResponse = await _client.PostAsync("/api/users", JsonContent.Create(newUser));
        createResponse.EnsureSuccessStatusCode();
        var createdUser = await createResponse.Content.ReadFromJsonAsync<UserDto>();

        var getResponse = await _client.GetAsync($"/api/users/{createdUser?.Id}");
        getResponse.EnsureSuccessStatusCode();
        var fetchedUser = await getResponse.Content.ReadFromJsonAsync<UserDto>();

        Assert.NotNull(fetchedUser);
        Assert.Equal(createdUser?.Id, fetchedUser.Id);
        Assert.Equal(createdUser?.Name, fetchedUser.Name);
        Assert.Equal(createdUser?.Email, fetchedUser.Email);
    }
}
