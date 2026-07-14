using SimpleStorage.DTO;
using SimpleStorage.Storage;

namespace SimpleStorage.Tests.Storage;

public sealed class SimpleStoreTests
{
    [Fact]
    public void Set_SuccessfulAddition_WhenStoreIsEmpty()
    {
        // Arrange
        var store = new SimpleStore();

        var key1 = "key1";
        var userProfile1 = CreateUserProfile(1);
        var key2 = "key2";
        var userProfile2 = CreateUserProfile(2);

        // Act
        store.Set(key1, userProfile1);
        store.Set(key2, userProfile2);

        // Assert
        var resultUser1 = store.Get(key1);
        var resultUser2 = store.Get(key2);

        Assert.NotNull(resultUser1);
        AssertUserProfilesEqual(userProfile1, resultUser1);

        Assert.NotNull(resultUser2);
        AssertUserProfilesEqual(userProfile2, resultUser2);
    }

    [Fact]
    public void Set_OverwriteValue_WhenExistingValue()
    {
        // Arrange
        var store = new SimpleStore();

        var key = "key1";
        var userProfile1 = CreateUserProfile(1);
        store.Set(key, userProfile1);

        var userProfile2 = CreateUserProfile(2);

        // Act
        store.Set(key, userProfile2);

        // Assert
        var resultUser = store.Get(key);
        Assert.NotNull(resultUser);

        AssertUserProfilesEqual(userProfile2, resultUser);
    }

    [Fact]
    public void Get_ReturnsNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var store = new SimpleStore();

        // Act
        var result = store.Get("key");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Delete_RemoveKeyAndValue()
    {
        // Arrange
        var store = new SimpleStore();

        var key = "key";
        var userProfile = CreateUserProfile(1);
        store.Set(key, userProfile);

        // Act
        store.Delete(key);

        // Assert
        var result = store.Get(key);
        Assert.Null(result);
    }

    [Fact]
    public async Task ParallelSetAndGet_SuccessfullySavesAndRetrievesDataAndCorrectStatistics()
    {
        var store = new SimpleStore();

        var totalTasks = 100;
        var setTasksCount = 60;
        var getTasksCount = totalTasks - setTasksCount;

        var tasks = new Task[totalTasks];

        // SET задачи
        for (var i = 0; i < setTasksCount; i++)
        {
            var index = i;
            tasks[i] = Task.Run(() =>
            {
                var key = $"user_{index}";
                var userProfile = CreateUserProfile(index);
                store.Set(key, userProfile);
            });
        }

        // GET задачи
        for (var i = setTasksCount; i < totalTasks; i++)
        {
            var index = i - setTasksCount;
            tasks[i] = Task.Run(() =>
            {
                var key = $"user_{index}";
                var user = store.Get(key);
                if (user != null)
                {
                    var expected = CreateUserProfile(index);
                    AssertUserProfilesEqual(expected, user);
                }
            });
        }

        await Task.WhenAll(tasks);

        // Проверяем статистику
        var (setCount, getCount, deleteCount) = store.GetStatistics();
        Assert.Equal(setTasksCount, setCount);
        Assert.Equal(getTasksCount, getCount);
        Assert.Equal(0, deleteCount);

        // Проверка данных по ключам
        for (var i = 0; i < setTasksCount; i++)
        {
            var key = $"user_{i}";
            var userProfile = store.Get(key);
            Assert.NotNull(userProfile);

            var expected = CreateUserProfile(i);
            AssertUserProfilesEqual(expected, userProfile);
        }
    }

    private static UserProfile CreateUserProfile(int id) =>
        new()
        {
            Id = id,
            UserName = $"User{id}",
            CreatedAt = DateTime.UtcNow
        };

    private static void AssertUserProfilesEqual(
        UserProfile expected,
        UserProfile actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.UserName, actual.UserName);
        Assert.Equal(expected.CreatedAt, actual.CreatedAt, TimeSpan.FromSeconds(1));
    }
}