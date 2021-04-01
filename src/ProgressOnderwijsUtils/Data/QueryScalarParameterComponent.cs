﻿using System;

namespace ProgressOnderwijsUtils
{
    struct QueryScalarParameterComponent : IQueryParameter
    {
        public object EquatableValue { get; private set; }

        public void ToSqlParameter(ref SqlParamArgs paramArgs)
            => paramArgs.Value = EquatableValue == CurrentTimeToken.Instance ? DateTime.Now
                : EquatableValue is ulong uint64val ? UInt64ToSqlBinary(uint64val)
                : EquatableValue is uint uint32val ? UInt32ToSqlBinary(uint32val)
                : EquatableValue;

        public static byte[] UInt64ToSqlBinary(ulong uint64val)
        {
            //https://stackoverflow.com/questions/19560436/bitwise-endian-swap-for-various-types
            uint64val = uint64val >> 32 | uint64val << 32;
            uint64val = (uint64val & 0xFFFF0000FFFF0000U) >> 16 | (uint64val & 0x0000FFFF0000FFFFU) << 16;
            uint64val = (uint64val & 0xFF00FF00FF00FF00U) >> 8 | (uint64val & 0x00FF00FF00FF00FFU) << 8;
            return BitConverter.GetBytes(uint64val);
        }

        public  static byte[] UInt32ToSqlBinary(uint uint32val)
        {
            uint32val = (uint32val & 0xFFFF0000U) >> 16 | (uint32val & 0x0000FFFFU) << 16;
            uint32val = (uint32val & 0xFF00FF00U) >> 8 | (uint32val & 0x00FF00FFU) << 8;
            return BitConverter.GetBytes(uint32val);
        }

        public static void AppendScalarParameter<TCommandFactory>(ref TCommandFactory factory, object? o)
            where TCommandFactory : struct, ICommandFactory
        {
            if (o == null || o == DBNull.Value) {
                ParameterizedSqlFactory.AppendSql(ref factory, "NULL");
            } else {
                var param = new QueryScalarParameterComponent { EquatableValue = o };
                ParameterizedSqlFactory.AppendSql(ref factory, factory.RegisterParameterAndGetName(param));
            }
        }
    }
}
