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
        var value1 = new byte[] { 1, 2, 3 };
        var key2 = "key2";
        var value2 = new byte[] { 4, 5, 6 };

        // Act
        store.Set(key1, value1);
        store.Set(key2, value2);

        // Assert
        Assert.Equal(value1, store.Get(key1));
        Assert.Equal(value2, store.Get(key2));
    }

    [Fact]
    public void Set_OverwriteValue_WhenExistingValue()
    {
        // Arrange
        var store = new SimpleStore();
        var key1 = "key1";
        var value1 = new byte[] { 1, 2, 3 };
        store.Set(key1, value1);

        var value2 = new byte[] { 4, 5, 6 };

        // Act
        store.Set(key1, value2);

        // Assert
        Assert.Equal(value2, store.Get(key1));
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
        var value = new byte[] { 1, 2, 3 };
        store.Set(key, value);

        // Act
        store.Delete(key);

        // Assert
        Assert.Null(store.Get(key));
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
                var key = $"key_{index}";
                var value = BitConverter.GetBytes(index);
                store.Set(key, value);
            });
        }

        // GET задачи
        for (var i = setTasksCount; i < totalTasks; i++)
        {
            var index = i - setTasksCount;
            tasks[i] = Task.Run(() =>
            {
                var key = $"key_{index}";
                var result = store.Get(key);
                if (result != null)
                {
                    var val = BitConverter.ToInt32(result, 0);
                    Assert.Equal(index, val);
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
            var key = $"key_{i}";
            var value = store.Get(key);
            Assert.NotNull(value);

            var val = BitConverter.ToInt32(value, 0);
            Assert.Equal(i, val);
        }
    }
}