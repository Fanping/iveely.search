namespace NDatabase.Api
{
    /// <summary>
    ///   Constants used for ordering queries and creating ordered collection iterators
    /// </summary>
    public sealed class OrderByConstants
    {
        private const int OrderByNoneType = 0;

        private const int OrderByDescType = 1;

        private const int OrderByAscType = 2;

        /// <summary>
        /// No order
        /// </summary>
        public static readonly OrderByConstants OrderByNone = new OrderByConstants(OrderByNoneType);

        /// <summary>
        /// Descending order
        /// </summary>
        public static readonly OrderByConstants OrderByDesc = new OrderByConstants(OrderByDescType);

        /// <summary>
        /// Ascending order
        /// </summary>
        public static readonly OrderByConstants OrderByAsc = new OrderByConstants(OrderByAscType);

        private readonly int _type;

        private OrderByConstants(int type)
        {
            _type = type;
        }

        /// <summary>
        /// Is descending order
        /// </summary>
        /// <returns>True if descending, false if not</returns>
        public bool IsOrderByDesc()
        {
            return _type == OrderByDescType;
        }

        /// <summary>
        /// Is ascending order
        /// </summary>
        /// <returns>True if ascending, false if not</returns>
        public bool IsOrderByAsc()
        {
            return _type == OrderByAscType;
        }

        /// <summary>
        /// Is no order set
        /// </summary>
        /// <returns>True if no order set, false if not</returns>
        public bool IsOrderByNone()
        {
            return _type == OrderByNoneType;
        }

        public override string ToString()
        {
            switch (_type)
            {
                case OrderByAscType:
                    return "order by asc";

                case OrderByDescType:
                    return "order by desc";

                default:
                    return "no order by";
            }
        }
    }
}
