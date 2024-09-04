using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Helper;

public class PaginationResult<T>
{
    public List<T> Data { get; set; }
    public Pager Pager { get; set; }
}