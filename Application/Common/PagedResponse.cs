namespace Application.Common;

/// <summary>
/// Response containing a page of results.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
/// <param name="Items">The items on the requested page.</param>
/// <param name="Page">The current page number.</param>
/// <param name="PageSize">The page size.</param>
/// <param name="TotalCount">The total number of matching items.</param>
public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount);
