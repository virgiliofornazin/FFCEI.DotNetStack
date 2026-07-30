namespace FFCEI.DotNetStack.Framework;

public static class PagingAlgorithms
{
    public static bool ComputePageIndexAndSize(ref int? pageIndex, ref int? pageSize, bool forcePaging = false, int defaultPageSize = 50)
    {
        if ((pageIndex != null && pageSize != null) || forcePaging)
        {
            pageIndex ??= 1;

            if (pageIndex < 1)
            {
                pageIndex = 1;
            }

            pageSize ??= defaultPageSize;

            if (pageSize < 1)
            {
                pageSize = 1;
            }

            return true;
        }

        return false;
    }

    public static bool ComputeSkipTake(int? pageIndex, int? pageSize, out int? skip, out int? take, bool forcePaging = false, int defaultPageSize = 50)
    {
        if (!ComputePageIndexAndSize(ref pageIndex, ref pageSize, forcePaging, defaultPageSize))
        {
            skip = null;
            take = null;

            return false;
        }

        skip = (pageIndex - 1) * pageSize;
        take = pageSize;

        return true;
    }

    public static bool ComputeStartIndexEndIndex(int? pageIndex, int? pageSize, out int? startIndex, out int? endIndex, bool forcePaging = false, int defaultPageSize = 50, bool zeroIndexing = true)
    {
        if (!ComputePageIndexAndSize(ref pageIndex, ref pageSize, forcePaging, defaultPageSize))
        {
            startIndex = null;
            endIndex = null;

            return false;
        }

        startIndex = (pageIndex - 1) * pageSize;

        if (!zeroIndexing)
        {
            ++startIndex;
        }

        endIndex = startIndex + (pageSize - 1);

        return true;
    }
}
