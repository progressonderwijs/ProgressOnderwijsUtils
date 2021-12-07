namespace ProgressOnderwijsUtils;

static class PooledSqlCommandAllocator
{
    static readonly int IndexCount = 129;
    static readonly int MaxArrayLength = IndexCount - 1;

    //Unfortunately, ConcurrentStacks and ConcurrentBags perform allocations when used in this fashion, and are thus unsuitable
    //conceptually, a ConcurrentBag that doesn't allocation on .Add(...) is what we're looking for here, and a queue is close enough.
    static readonly ConcurrentQueue<SqlCommand>[] bagsByIndex = InitBags();

    static ConcurrentQueue<SqlCommand>[] InitBags()
    {
        var allBags = new ConcurrentQueue<SqlCommand>[IndexCount];
        for (var i = 0; i < IndexCount; i++) {
            allBags[i] = new();
        }
        return allBags;
    }

    public static SqlCommand GetByLength(int length)
    {
        if (length <= MaxArrayLength) {
            var bag = bagsByIndex[length];
            if (bag.TryDequeue(out var result)) {
                return result;
            }
        }
        var cmd = new SqlCommand { EnableOptimizedParameterBinding = true, };
        var parameters = new SqlParameter[length];
        for (var i = 0; i < length; i++) {
            parameters[i] = new() { ParameterName = CommandFactory.IndexToParameterName(i), };
        }
        cmd.Parameters.AddRange(parameters);
        return cmd;
    }

    public static void ReturnToPool(SqlCommand cmd)
    {
        var parameters = cmd.Parameters;
        var parameterCount = parameters.Count;
        if (parameterCount > MaxArrayLength) {
            return;
        }
        var bag = bagsByIndex[parameterCount];
        for (var i = 0; i < parameterCount; i++) {
            parameters[i].Value = null;
            parameters[i].TypeName = null;
        }
        cmd.CommandText = null;
        cmd.Connection = null;
        bag.Enqueue(cmd);
    }
}
