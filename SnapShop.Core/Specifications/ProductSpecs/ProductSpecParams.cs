using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnapShop.Core.Specifications.ProductSpecs
{
    public class ProductSpecParams
    {
        private const int MaxPageSize = 10;
        private int pageSize;

        public int PageSize
        {
            get => pageSize == 0 ? 6 : pageSize; // default if not set
            set => pageSize = (value <= 0) ? 6 : (value > MaxPageSize ? MaxPageSize : value);
        }


        public int PageIndex { get; set; } = 1;

        private string? search;

        public string? Search
        {
            get { return search; }
            set { search = value?.ToLower(); }
        }

        public string? sort { get; set; }
        public int? brandId { get; set; }
        public int? categoryId { get; set; }
    }
}
