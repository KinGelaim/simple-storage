namespace SimpleStorage.Tests;

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
        var value2= new byte[] { 4, 5, 6 };

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
}