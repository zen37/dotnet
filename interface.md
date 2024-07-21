### Generic vs. Non-Generic Interfaces

#### **Generic Interface (`IStorage<T>`)**

1. **Type Safety**: Generics provide compile-time type safety. You can ensure that the methods operate on the correct type, reducing runtime errors and avoiding type casting.
   
2. **Flexibility**: Each storage implementation can be tailored for different types. For example, `Product`, `Customer`, and `Order` can each have their own storage implementations without mixing types.

3. **Code Reusability**: A single generic interface can be used across various types, promoting reusability and reducing code duplication.

4. **Type-specific Operations**: Operations on entities often need to be type-specific, such as handling different properties or IDs. With generics, you can work with the exact type in methods like `AddAsync`, `UpdateAsync`, etc.

#### **Non-Generic Interface (`IStorage`)**

1. **Lack of Type Safety**: Without generics, the interface can't specify the type of entity it operates on. This means you would need to use `object` or some base type, which introduces casting and runtime errors.

2. **Loss of Specificity**: All methods would have to work with `object` or a base class. This means you can't directly enforce constraints on types, leading to less predictable behavior.

3. **Complex Implementations**: Implementations would need to handle multiple types through casting or reflection, making them more complex and error-prone.

4. **No Compile-time Checks**: You lose the benefits of compile-time type checking, increasing the risk of runtime errors and reducing code clarity.

### Example of a Non-Generic Interface

If you still want to use a non-generic interface, you would define it like this:

```csharp
public interface IStorage
{
    Task<IEnumerable<object>> GetAllAsync();
    Task<object> GetByIdAsync(int id);
    Task AddAsync(object entity);
    Task UpdateAsync(object entity);
    Task DeleteAsync(int id);
}
```

#### Issues with Non-Generic Interface:

1. **Casting Required**: You would need to cast objects to the appropriate type, which can lead to runtime errors if the types don't match.

   ```csharp
   public class FileStorage : IStorage
   {
       // Assuming a specific type is used, e.g., Product
       public async Task<IEnumerable<object>> GetAllAsync()
       {
           var data = await File.ReadAllTextAsync("filePath");
           return JsonSerializer.Deserialize<IEnumerable<Product>>(data) ?? Enumerable.Empty<Product>();
       }

       public async Task<object> GetByIdAsync(int id)
       {
           var items = await GetAllAsync();
           return items.FirstOrDefault(item => ((Product)item).Id == id);
       }

       public async Task AddAsync(object entity)
       {
           var items = (await GetAllAsync()).Cast<Product>().ToList();
           items.Add((Product)entity);
           await SaveAllAsync(items);
       }

       public async Task UpdateAsync(object entity)
       {
           var items = (await GetAllAsync()).Cast<Product>().ToList();
           var index = items.FindIndex(item => item.Id == ((Product)entity).Id);
           if (index != -1)
           {
               items[index] = (Product)entity;
               await SaveAllAsync(items);
           }
       }

       public async Task DeleteAsync(int id)
       {
           var items = (await GetAllAsync()).Cast<Product>().ToList();
           var item = items.FirstOrDefault(i => i.Id == id);
           if (item != null)
           {
               items.Remove(item);
               await SaveAllAsync(items);
           }
       }

       private async Task SaveAllAsync(IEnumerable<Product> items)
       {
           var data = JsonSerializer.Serialize(items);
           await File.WriteAllTextAsync("filePath", data);
       }
   }
   ```

2. **Implementation Complexity**: Implementations become more complex because you need to handle different types at runtime, which can lead to bugs and maintenance issues.

### Conclusion

Using a generic interface like `IStorage<T>` provides type safety, flexibility, and simplifies implementation. It ensures that all operations are type-specific, reducing the risk of runtime errors and making the code easier to understand and maintain.

A non-generic `IStorage` interface could work in scenarios where all types are treated uniformly, but it’s generally less flexible and more error-prone. For most cases, sticking with a generic approach is the better choice for clarity and safety.