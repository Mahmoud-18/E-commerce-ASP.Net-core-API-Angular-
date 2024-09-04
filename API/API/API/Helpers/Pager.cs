using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Helper;

public class Pager
{
    private const int MaxPageSize = 60;
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; } = 1;

    private int _pageSize;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize ) ? MaxPageSize : value;
    }

    public int TotalPages { get; set; }
    public int StartPage { get; set; }
    public int EndPage { get; set; }

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    public Pager() { }
    public Pager(int totalItems, int Page, int pageSize = 12)
    {
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        int currentPage = Page;

        int startPage = currentPage - 5;
        int endPage = currentPage + 4;

        if (startPage <= 0)
        {
            endPage = endPage - (startPage - 1);
            startPage = 1;
        }

        if (endPage > totalPages)
        {
            endPage = totalPages;
            if (endPage > 10)
            {
                startPage = endPage - 9;
            }
        }
        TotalItems = totalItems;
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalPages = totalPages;
        StartPage = startPage;
        EndPage = endPage;
    }
}
