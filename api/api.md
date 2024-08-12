#Organizing ASP.NET Core Minimal APIs
https://www.tessferrandez.com/blog/2023/10/31/organizing-minimal-apis.html

# Tutorial: Create a minimal API with ASP.NET Core
https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-8.0&tabs=visual-studio

https://github.com/sswietoniowski/learning-aspnetcore-webapi-7-building-minimal-apis/tree/master/contacts

https://github.com/dotnet/AspNetCore.Docs.Samples/tree/main/fundamentals/minimal-apis/samples



# Filters in Minimal API apps

https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/min-api-filters?view=aspnetcore-8.0

# Validation

Both approaches have their own advantages and can be chosen based on your specific needs and preferences. Here's a comparison of the two approaches:

### Using `.AddFilter<BasicValidator<CouponCreateDTO>>`:

**Pros:**
1. **Separation of Concerns**: Validation logic is separated from the main handler logic, making your handler methods cleaner and more focused on their primary responsibilities.
2. **Reusability**: The `BasicValidator<T>` can be reused across different endpoints and different DTOs, providing a consistent validation mechanism.
3. **Centralized Error Handling**: The `BasicValidator<T>` ensures that all validation errors are handled in a consistent manner, reducing the likelihood of duplicated error handling logic.

**Cons:**
1. **Complexity**: Adding custom filters can increase the complexity of the routing setup and might require additional configuration.
2. **Invisibility**: The validation logic is not directly visible in the handler method, which might make the code less straightforward for new developers.

**Example Setup:**

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();
    
    // Register FluentValidation
    services.AddFluentValidation(fv => 
    {
        fv.RegisterValidatorsFromAssemblyContaining<CouponCreateValidation>();
    });

    // Register the BasicValidator as a route handler filter
    services.AddScoped(typeof(BasicValidator<>));
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseRouting();
    app.UseAuthorization();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapPost("/api/coupon", async context =>
        {
            var validator = context.RequestServices.GetService<BasicValidator<CouponCreateDTO>>();
            await validator.InvokeAsync(context, async ctx =>
            {
                var couponRepo = ctx.RequestServices.GetService<ICouponRepository>();
                var mapper = ctx.RequestServices.GetService<IMapper>();
                var couponCreateDTO = ctx.GetArgument<CouponCreateDTO>(0);

                return await CreateCoupon(couponRepo, mapper, couponCreateDTO);
            });
        });
    });
}
```

### Direct Validation in Endpoint:

**Pros:**
1. **Simplicity**: Validation logic is directly included in the handler method, making the flow straightforward and easy to understand.
2. **Visibility**: The validation logic is visible directly within the method, making it easier to see and understand what the method does at a glance.

**Cons:**
1. **Code Duplication**: If multiple endpoints require similar validation logic, you might end up duplicating validation code across different handler methods.
2. **Responsibility Overload**: The handler method becomes responsible for both business logic and validation, potentially violating the single responsibility principle.

**Example Setup:**

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseRouting();
    app.UseAuthorization();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapPost("/api/coupon", async (IMapper _mapper, IValidator<CouponCreateDTO> _validation, [FromBody] CouponCreateDTO coupon_createDTO) => 
        {
            APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

            var validationResult = await _validation.ValidateAsync(coupon_createDTO);

            if (!validationResult.IsValid)
            {
                response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
                return Results.BadRequest(response);
            }

            // Assuming _couponRepo is resolved from context or passed as an argument
            var _couponRepo = ...;
            if (_couponRepo.GetAsync(coupon_createDTO.Name).GetAwaiter().GetResult() != null)
            {
                response.ErrorMessages.Add("Coupon Name already Exists");
                return Results.BadRequest(response);
            }

            Coupon coupon = _mapper.Map<Coupon>(coupon_createDTO);
            await _couponRepo.CreateAsync(coupon);
            await _couponRepo.SaveAsync();
            CouponDTO couponDTO = _mapper.Map<CouponDTO>(coupon);

            response.Result = couponDTO;
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.Created;
            return Results.Ok(response);
        });
    });
}
```

### Conclusion

- If you prefer a clean separation of concerns and plan to reuse the validation logic across multiple endpoints, using `.AddFilter<BasicValidator<CouponCreateDTO>>` is a better approach.
- If you prefer simplicity and having the validation logic visible directly in the handler method, then integrating the validation directly in the endpoint might be more suitable.

Choose the approach that best aligns with your project's structure and maintainability goals.


The `CreateHeader` method itself does not need to handle JSON deserialization issues if you use middleware to handle such cases. Here’s how the responsibilities are divided between middleware and your endpoint method:

### Middleware vs. Endpoint Method

1. **Middleware**:
   - **Purpose**: Middleware like `JsonExceptionMiddleware` captures issues related to malformed JSON, such as syntax errors in the request body, and returns a user-friendly error message.
   - **Example**: The middleware catches `JsonException` and returns a `400 Bad Request` with an appropriate message if the JSON cannot be deserialized.

2. **Endpoint Method**:
   - **Purpose**: The endpoint method handles validation and business logic. It uses FluentValidation to ensure the data meets your application's rules and constraints.
   - **Example**: The `CreateHeader` method checks the validity of the `HeaderDTO` properties according to the rules defined in the `HeaderCreateValidation` class.

### Combined Approach

1. **Middleware Setup**:
   Middleware should be set up in your `Program.cs` to handle malformed JSON globally. This way, any request with invalid JSON will be intercepted and responded to with an appropriate error message before it reaches your endpoint methods.


2. **Endpoint Method**:
   The endpoint method should focus on validating the deserialized DTO and processing the valid data. Since malformed JSON will be handled by the middleware, the endpoint method can assume that the input is a valid JSON that has been correctly deserialized.



### Summary

- **Middleware** handles issues with JSON deserialization and prevents malformed requests from reaching your endpoint methods.
- **Endpoint methods** handle validation of the deserialized data and application-specific logic.

It seems like the `JsonExceptionMiddleware` is not catching the `BadHttpRequestException` that occurs during JSON deserialization. This issue might be because the `JsonExceptionMiddleware` is registered too late in the middleware pipeline or because the exception is raised before the middleware has a chance to handle it.

### Here’s how to address this issue:

1. **Ensure Middleware Registration Order**

   Middleware needs to be registered in the correct order in the pipeline. Make sure that `app.UseMiddleware<JsonExceptionMiddleware>()` is placed before `app.UseRouting()` and `app.UseEndpoints()`.

  

2. **Handle Malformed JSON Globally**

   The `JsonExceptionMiddleware` should catch and handle `JsonException` globally. However, if the exception is raised before it reaches the middleware, you can use a global exception handler.

   Here’s an example of a global exception handler that catches all exceptions, including those thrown during JSON deserialization:

   ```csharp
   public class ExceptionHandlingMiddleware
   {
       private readonly RequestDelegate _next;

       public ExceptionHandlingMiddleware(RequestDelegate next)
       {
           _next = next;
       }

       public async Task InvokeAsync(HttpContext context)
       {
           try
           {
               await _next(context);
           }
           catch (JsonException ex)
           {
               context.Response.StatusCode = StatusCodes.Status400BadRequest;
               context.Response.ContentType = "application/json";
               var result = JsonSerializer.Serialize(new { error = "Invalid JSON format: " + ex.Message });
               await context.Response.WriteAsync(result);
           }
           catch (Exception ex)
           {
               context.Response.StatusCode = StatusCodes.Status500InternalServerError;
               context.Response.ContentType = "application/json";
               var result = JsonSerializer.Serialize(new { error = "An unexpected error occurred: " + ex.Message });
               await context.Response.WriteAsync(result);
           }
       }
   }
   ```

   Register this middleware in place of or in addition to `JsonExceptionMiddleware`:

   ```csharp
   // Register global exception handling middleware
   app.UseMiddleware<ExceptionHandlingMiddleware>();
   ```

3. **Verify Exception Details**

   Ensure the exception details are being correctly logged or displayed. If using logging, ensure it captures `BadHttpRequestException` and `JsonException` properly.

4. **Test with Valid and Invalid JSON**

   Make sure to test with valid and invalid JSON payloads to ensure the middleware behaves as expected:


### Summary

1. **Middleware Ordering**: Ensure the middleware is registered before routing and endpoint configuration.
2. **Global Exception Handling**: Use a general exception handling middleware to catch all exceptions, including those from JSON deserialization.
3. **Test and Log**: Verify behavior with valid and invalid JSON and ensure proper logging of exceptions.

By following these steps, you should be able to capture and handle JSON deserialization errors effectively in your ASP.NET Core application.

# StatusCodes vs Numeric Values

The difference between using `StatusCodes.Status201Created` and `201` (as well as `StatusCodes.Status400BadRequest` vs `400`) lies mainly in readability, maintainability, and consistency of your code. Here’s a breakdown of the advantages:

### Using `StatusCodes.Status201Created` and `StatusCodes.Status400BadRequest`

1. **Readability**:
   - **Self-Explanatory**: `StatusCodes.Status201Created` is more descriptive and self-explanatory than `201`. It immediately conveys what the status code represents without having to look up the numeric value.

2. **Maintainability**:
   - **Avoid Hard-Coding**: Using named constants like `StatusCodes.Status201Created` helps avoid magic numbers in your code, which can be hard to understand and maintain. If the status codes change or if you need to update them, you’ll only need to make changes in one place.
   - **Consistency**: It’s easier to maintain consistency across your application when you use these named constants. It reduces the likelihood of errors when manually typing numeric values.

3. **IntelliSense Support**:
   - **IDE Assistance**: Named constants are easier to work with in most IDEs because they can provide IntelliSense or code suggestions. This helps in reducing errors and speeds up development.

### Using Numeric Values Directly (`201` and `400`)

1. **Simplicity**:
   - **Shorter Code**: Using numeric values makes the code slightly shorter. However, this minor gain in brevity is usually outweighed by the benefits of clarity and maintainability.

2. **Potential for Errors**:
   - **Less Clear**: Numeric values can be less clear to someone reading the code, especially if they are unfamiliar with the specific status codes used.

### Best Practices

In general, using named constants like `StatusCodes.Status201Created` is considered a best practice for the following reasons:

- **Code Clarity**: Named constants make your code more readable and self-explanatory.
- **Maintainability**: It’s easier to update or change status codes if you use named constants.
- **Consistency**: It ensures that your code is consistent and reduces the risk of errors.

### Example

Here’s how it looks using named constants:

```csharp
app.MapPost("/api/subitem", CreateSubItem)
    .WithName("CreateSubItem")
    .Accepts<SubItemCreateDTO>("application/json")
    .Produces(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest);
```

And using numeric values:

```csharp
app.MapPost("/api/subitem", CreateSubItem)
    .WithName("CreateSubItem")
    .Accepts<SubItemCreateDTO>("application/json")
    .Produces(201)
    .Produces(400);
```

While both approaches are functionally equivalent, using named constants aligns with best practices for clearer, more maintainable code.

# URL Parameters

In RESTful API design, the best practice is to pass the resource identifier (like `id`) as a URL parameter rather than in the JSON body for GET, PUT, and DELETE requests. This approach has several advantages:

### Advantages of Using URL Parameters for `id`

1. **Clarity and Semantics**:
   - Using the URL to specify the resource identifier makes it clear which resource is being acted upon. It aligns with the principles of REST where URLs represent resources.

2. **Conformity with HTTP Methods**:
   - GET requests should be idempotent and typically do not include a body. Including `id` in the URL adheres to this convention.
   - PUT and DELETE requests also often have `id` in the URL to specify the resource being modified or deleted.

3. **Cacheability**:
   - URLs with resource identifiers can be more easily cached by intermediaries like proxies and CDNs.

4. **Simplicity**:
   - It's simpler and more consistent to always know that identifiers for resources are passed in the URL.

### Example Implementation

#### Route Definition
Here’s how you can define routes with `id` in the URL in ASP.NET Core:

```csharp
app.MapGet("/api/Header/{id:guid}", async (IService<Header> HeaderService, Guid id) =>
{
    try
    {
        var Header = await HeaderService.GetByIdAsync(id);
        return Header != null ? Results.Ok(Header) : Results.NotFound();
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
}).WithName("GetHeader");
```

#### DTO Definitions
For POST and PUT requests, you can still use DTOs without the `id` in the body:

```csharp
public class HeaderCreateDTO
{
    public string Name { get; set; }
    public int CapacityInAmps { get; set; }
}

public class HeaderUpdateDTO
{
    public int CapacityInAmps { get; set; }
}
```

#### Validators
Validators should focus on the properties in the DTO:

```csharp
public class HeaderCreateValidator : AbstractValidator<HeaderCreateDTO>
{
    public HeaderCreateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Header name is required.");
        RuleFor(x => x.CapacityInAmps).GreaterThan(0).WithMessage("Capacity in Amps must be greater than zero.");
    }
}

public class HeaderUpdateValidator : AbstractValidator<HeaderUpdateDTO>
{
    public HeaderUpdateValidator()
    {
        RuleFor(x => x.CapacityInAmps).GreaterThan(0).WithMessage("Capacity in Amps must be greater than zero.");
    }
}
```

#### Controller Methods
For completeness, here’s how you can define POST and PUT methods:

```csharp
app.MapPost("/api/Header", async (IService<Header> HeaderService, HeaderCreateDTO HeaderDto) =>
{
    try
    {
        var newHeader = new Header
        {
            Id = Guid.NewGuid(),
            Name = HeaderDto.Name,
            CapacityInAmps = HeaderDto.CapacityInAmps
        };

        await HeaderService.AddAsync(newHeader);
        return Results.Created($"/api/Header/{newHeader.Id}", newHeader);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
}).WithName("CreateHeader");

app.MapPut("/api/Header/{id:guid}", async (IService<Header> HeaderService, Guid id, HeaderUpdateDTO HeaderDto) =>
{
    try
    {
        var existingHeader = await HeaderService.GetByIdAsync(id);
        if (existingHeader == null)
        {
            return Results.NotFound();
        }

        existingHeader.CapacityInAmps = HeaderDto.CapacityInAmps;
        await HeaderService.UpdateAsync(existingHeader);
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
}).WithName("UpdateHeader");
```

### Conclusion

Passing `id` as a URL parameter is more consistent with RESTful principles, makes your API clearer, and better aligns with HTTP conventions. It simplifies your request handling and improves the semantic clarity of your API endpoints.


# Methods/Verbs

When deciding between `PUT` and `PATCH` for updating an entity via an API, it's important to understand the semantic differences between these HTTP methods:

1. **`PUT`**:
   - **Full Update**: The `PUT` method is typically used to update an entire resource. When you use `PUT`, the client sends a complete representation of the resource.
   - **Idempotent**: Subsequent `PUT` requests with the same data should have the same effect as the first one.
   - **Replaces Entire Resource**: The `PUT` request replaces the entire resource with the data provided.

2. **`PATCH`**:
   - **Partial Update**: The `PATCH` method is used for partial updates. When you use `PATCH`, the client sends only the fields that need to be updated, not the entire resource.
   - **Not Necessarily Idempotent**: While `PATCH` can be idempotent, it often isn't by nature. Each `PATCH` request might change the resource in a different way depending on its current state.
   - **More Flexible**: `PATCH` allows for more granular updates, which can be more efficient in terms of data transmission and processing.

### Choosing Between `PUT` and `PATCH`

- **Use `PUT` if**:
  - You need to replace the entire resource with the updated content.
  - The client can provide a complete representation of the resource.

- **Use `PATCH` if**:
  - You need to update only specific fields of the resource.
  - The changes are more granular and don't require sending the entire resource representation.

### Example Usage

Here's an example of how you might implement `PUT` and `PATCH` in your ASP.NET Core API.

#### PUT Example

```csharp
[HttpPut("{id}")]
public async Task<IActionResult> UpdateHeader(Guid id, [FromBody] Header Header, [FromServices] ILogger<Program> logger)
{
    try
    {
        if (id != Header.Id)
        {
            return BadRequest("ID mismatch");
        }

        var existingHeader = await HeaderService.GetByIdAsync(id);
        if (existingHeader == null)
        {
            return NotFound();
        }

        // Update the entire Header entity
        await HeaderService.UpdateAsync(Header);
        
        return NoContent();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error occurred while updating Header with ID: {Id}", id);
        return Problem(ex.Message);
    }
}
```

#### PATCH Example

```csharp
[HttpPatch("{id}")]
public async Task<IActionResult> PartialUpdateHeader(Guid id, [FromBody] JsonPatchDocument<Header> patchDoc, [FromServices] ILogger<Program> logger)
{
    try
    {
        if (patchDoc == null)
        {
            return BadRequest();
        }

        var existingHeader = await HeaderService.GetByIdAsync(id);
        if (existingHeader == null)
        {
            return NotFound();
        }

        patchDoc.ApplyTo(existingHeader, ModelState);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await HeaderService.UpdateAsync(existingHeader);

        return NoContent();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error occurred while partially updating Header with ID: {Id}", id);
        return Problem(ex.Message);
    }
}
```

### Summary
- Use `PUT` when you need to replace an entire resource.
- Use `PATCH` when you need to update part of a resource.
- Both methods can be implemented to provide flexible and efficient updates depending on the requirements of your API.

The recommendation to use `PUT` for update operations generally stems from the principle of simplicity and consistency in APIs. Using `PUT` ensures that updates are clear and the entire resource is being replaced, which can simplify both client and server logic. However, it's important to understand the nuances and choose the method that best fits your specific use case. 

### Why Use `PUT` for Update Operations

1. **Simplicity**: With `PUT`, the client sends the entire resource, making the update operation straightforward. There’s no ambiguity about what fields are being updated.
2. **Idempotence**: `PUT` is idempotent, meaning that making the same `PUT` request multiple times will have the same effect as making it once. This can simplify error handling and retries.
3. **Consistency**: Using `PUT` for updates can make your API consistent, especially if you are always dealing with complete resources.

### When `PATCH` Might Be More Appropriate

1. **Efficiency**: If only a small part of the resource needs updating, `PATCH` can be more efficient as it avoids sending the entire resource.
2. **Partial Updates**: When the update involves only a few fields, `PATCH` is more appropriate and can reduce the payload size and processing overhead.


### Summary

- **Use `PUT`** if your updates usually involve replacing the entire resource or if you want the simplicity and idempotence it offers.
- **Use `PATCH`** if your updates are typically partial and you want to optimize for smaller payloads and more efficient updates.

Both methods have their place, and choosing the right one depends on your specific use case and the nature of the updates you need to perform.

##