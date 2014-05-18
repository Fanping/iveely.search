using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iveely.Remote.Commands
{
    public class CommandCode
    {
        public const int UNDEFINED = 0;

        // XTable
        public const int REPLACE = 1;
        public const int DELETE = 2;
        public const int DELETE_RANGE = 3;
        public const int INSERT_OR_IGNORE = 4;
        public const int CLEAR = 5;

        public const int TRY_GET = 6;
        public const int FORWARD = 7;
        public const int BACKWARD = 8;
        public const int FIND_NEXT = 9;
        public const int FIND_AFTER = 10;
        public const int FIND_PREV = 11;
        public const int FIND_BEFORE = 12;
        public const int FIRST_ROW = 13;
        public const int LAST_ROW = 14;
        public const int COUNT = 15;

        public const int XTABLE_DESCRIPTOR_GET = 16;
        public const int XTABLE_DESCRIPTOR_SET = 17;

        // Storage engine
        public const int STORAGE_ENGINE_COMMIT = 22;
        public const int STORAGE_ENGINE_GET_ENUMERATOR = 23;
        public const int STORAGE_ENGINE_RENAME = 24;
        public const int STORAGE_ENGINE_EXISTS = 25;
        public const int STORAGE_ENGINE_FIND_BY_NAME = 26;
        public const int STORAGE_ENGINE_FIND_BY_ID = 27;
        public const int STORAGE_ENGINE_OPEN_XTABLE = 28;
        public const int STORAGE_ENGINE_OPEN_XFILE = 29;
        public const int STORAGE_ENGINE_DELETE = 30;
        public const int STORAGE_ENGINE_COUNT = 31;
        public const int STORAGE_ENGINE_DESCRIPTOR = 32;
        public const int STORAGE_ENGINE_GET_CACHE_SIZE = 33;
        public const int STORAGE_ENGINE_SET_CACHE_SIZE = 34;

        //Heap
        public const int HEAP_OBTAIN_NEW_HANDLE = 40;
        public const int HEAP_RELEASE_HANDLE = 41;
        public const int HEAP_EXISTS_HANDLE = 42;
        public const int HEAP_WRITE = 43;
        public const int HEAP_READ = 44;
        public const int HEAP_COMMIT = 45;
        public const int HEAP_CLOSE = 46;
        public const int HEAP_GET_TAG = 47;
        public const int HEAP_SET_TAG = 48;
        public const int HEAP_DATA_SIZE = 49;
        public const int HEAP_SIZE = 50;

        public const int EXCEPTION = 63;
        public const int MAX = 64;
    }
}
