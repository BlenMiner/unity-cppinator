#ifndef UNITYINTEROP_H
#define UNITYINTEROP_H

#include <unityengine.h>
#include <cstddef>
#include <stdint.h>

template<size_t N, typename... Args>
struct NthTypeOf;

// Base case for the first type in the list.
template<typename T, typename... Args>
struct NthTypeOf<0, T, Args...> {
    using type = T;
};

// Recursive specialization to count down to the nth type.
template<size_t N, typename T, typename... Args>
struct NthTypeOf<N, T, Args...> {
    using type = typename NthTypeOf<N-1, Args...>::type;
};

template<typename T, typename... Args>
struct SizeOfArgs {
    static constexpr int64_t size = sizeof(T) + SizeOfArgs<Args...>::size;
    static constexpr int64_t count = 1 + SizeOfArgs<Args...>::count; // Count the arguments
}; 

template<typename T>
struct SizeOfArgs<T> {
    static constexpr int64_t size = sizeof(T);
    static constexpr int64_t count = 1; // Base case, one type, count is one
};

template<>
struct SizeOfArgs<void> {
    static constexpr int64_t size = 0;
    static constexpr int64_t count = 0;
};

template<typename R, typename... Args>
struct FunctionTraits {
    static constexpr int64_t argsSize() {
        return SizeOfArgs<Args...>::size;
    }

    static constexpr int64_t returnSize() {
        return returnSizeHelper(std::is_void<R>());
    }

    static constexpr int64_t returnSizeHelper(std::true_type) {
        return 0;  // Return 0 or any other suitable value for `void`
    }

    static constexpr int64_t returnSizeHelper(std::false_type) {
        return sizeof(R);  // Only valid when R is not void
    }

    static constexpr int64_t argsCount() {
        return SizeOfArgs<Args...>::count;
    } 
 
    template<int64_t N>
    static constexpr int64_t argSizeAt() {
        return N < SizeOfArgs<Args...>::count ? SizeOfArgs<typename NthTypeOf<N, Args...>::type>::size : 0;
    }
};

#define EXPAND(x) x

#define _GLUE(X,Y) X##Y
#define GLUE(X,Y) _GLUE(X,Y)
 
/* Returns the 100th argument. */
#define _ARG_100(_,\
   _100,_99,_98,_97,_96,_95,_94,_93,_92,_91,_90,_89,_88,_87,_86,_85,_84,_83,_82,_81, \
   _80,_79,_78,_77,_76,_75,_74,_73,_72,_71,_70,_69,_68,_67,_66,_65,_64,_63,_62,_61, \
   _60,_59,_58,_57,_56,_55,_54,_53,_52,_51,_50,_49,_48,_47,_46,_45,_44,_43,_42,_41, \
   _40,_39,_38,_37,_36,_35,_34,_33,_32,_31,_30,_29,_28,_27,_26,_25,_24,_23,_22,_21, \
   _20,_19,_18,_17,_16,_15,_14,_13,_12,_11,_10,_9,_8,_7,_6,_5,_4,_3,_2,X_,...) X_

/* Returns whether __VA_ARGS__ has a comma (up to 100 arguments). */
#define HAS_COMMA(...) EXPAND(_ARG_100(__VA_ARGS__, \
   1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, \
   1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, \
   1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, \
   1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 ,1, \
   1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0))

/* Produces a comma if followed by a parenthesis. */
#define _TRIGGER_PARENTHESIS_(...) ,
#define _PASTE5(_0, _1, _2, _3, _4) _0 ## _1 ## _2 ## _3 ## _4
#define _IS_EMPTY_CASE_0001 ,
/* Returns true if inputs expand to (false, false, false, true) */
#define _IS_EMPTY(_0, _1, _2, _3) HAS_COMMA(_PASTE5(_IS_EMPTY_CASE_, _0, _1, _2, _3))
/* Returns whether __VA_ARGS__ is empty. */
#define IS_EMPTY(...)                                               \
   _IS_EMPTY(                                                       \
      HAS_COMMA(__VA_ARGS__),                                       \
      HAS_COMMA(_TRIGGER_PARENTHESIS_ __VA_ARGS__),                 \
      HAS_COMMA(__VA_ARGS__ (/*empty*/)),                           \
      HAS_COMMA(_TRIGGER_PARENTHESIS_ __VA_ARGS__ (/*empty*/))      \
   )

#define _VAR_COUNT_EMPTY_1(...) 0
#define _VAR_COUNT_EMPTY_0(...) EXPAND(_ARG_100(__VA_ARGS__, \
   100,99,98,97,96,95,94,93,92,91,90,89,88,87,86,85,84,83,82,81, \
   80,79,78,77,76,75,74,73,72,71,70,69,68,67,66,65,64,63,62,61, \
   60,59,58,57,56,55,54,53,52,51,50,49,48,47,46,45,44,43,42,41, \
   40,39,38,37,36,35,34,33,32,31,30,29,28,27,26,25,24,23,22,21, \
   20,19,18,17,16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1))
#define VAR_COUNT(...) GLUE(_VAR_COUNT_EMPTY_, IS_EMPTY(__VA_ARGS__))(__VA_ARGS__)

#define ARGUMENTS_0(t) void
#define ARGUMENTS_2(t1, n1) t1 n1
#define ARGUMENTS_4(t1, n1, t2, n2) t1 n1, t2 n2
#define ARGUMENTS_6(t1, n1, t2, n2, t3, n3) t1 n1, t2 n2, t3 n3
#define ARGUMENTS_8(t1, n1, t2, n2, t3, n3, t4, n4) t1 n1, t2 n2, t3 n3, t4 n4
#define ARGUMENTS_10(t1, n1, t2, n2, t3, n3, t4, n4, t5, n5) t1 n1, t2 n2, t3 n3, t4 n4, t5 n5
#define ARGUMENTS_12(t1, n1, t2, n2, t3, n3, t4, n4, t5, n5, t6, n6) t1 n1, t2 n2, t3 n3, t4 n4, t5 n5, t6 n6

#define ARGUMENTS_TYPES_0(t) void
#define ARGUMENTS_TYPES_2(t1, n1) t1
#define ARGUMENTS_TYPES_4(t1, n1, t2, n2) t1, t2
#define ARGUMENTS_TYPES_6(t1, n1, t2, n2, t3, n3) t1, t2, t3
#define ARGUMENTS_TYPES_8(t1, n1, t2, n2, t3, n3, t4, n4) t1, t2, t3, t4
#define ARGUMENTS_TYPES_10(t1, n1, t2, n2, t3, n3, t4, n4, t5, n5) t1, t2, t3, t4, t5
#define ARGUMENTS_TYPES_12(t1, n1, t2, n2, t3, n3, t4, n4, t5, n5, t6, n6) t1, t2, t3, t4, t5, t6

#define CONCATENATE_IMPL(s1, s2) s1 ## s2
#define CONCATENATE(s1, s2) CONCATENATE_IMPL(s1, s2)

#define _INTERNAL_ARGS(...) CONCATENATE(ARGUMENTS_, VAR_COUNT(__VA_ARGS__))(__VA_ARGS__)
#define _INTERNAL_ARGS_TYPES(...) CONCATENATE(ARGUMENTS_TYPES_, VAR_COUNT(__VA_ARGS__))(__VA_ARGS__)

// Size of the arguments ================================================

#define ARG_SIZE_FUNC_0

#define ARG_SIZE_FUNC_1(name, ret, ...) \
    EXPORT(int64_t) GENERATED_##name##_arg0Size() { return FunctionTraits<ret, __VA_ARGS__>::argSizeAt<0>(); }

#define ARG_SIZE_FUNC_2(name, ret, ...) \
    ARG_SIZE_FUNC_1(name, ret, __VA_ARGS__) \
    EXPORT(int64_t) GENERATED_##name##_arg1Size() { return FunctionTraits<ret, __VA_ARGS__>::argSizeAt<1>(); }

#define ARG_SIZE_FUNC_3(name, ret, ...) \
    ARG_SIZE_FUNC_2(name, ret, __VA_ARGS__) \
    EXPORT(int64_t) GENERATED_##name##_arg2Size() { return FunctionTraits<ret, __VA_ARGS__>::argSizeAt<2>(); }

#define ARG_SIZE_FUNC_4(name, ret, ...) \
    ARG_SIZE_FUNC_3(name, ret, __VA_ARGS__) \
    EXPORT(int64_t) GENERATED_##name##_arg3Size() { return FunctionTraits<ret, __VA_ARGS__>::argSizeAt<3>(); }

#define ARG_SIZE_FUNC_5(name, ret, ...) \
    ARG_SIZE_FUNC_4(name, ret, __VA_ARGS__) \
    EXPORT(int64_t) GENERATED_##name##_arg4Size() { return FunctionTraits<ret, __VA_ARGS__>::argSizeAt<4>(); }

#define ARG_SIZE_FUNC_6(name, ret, ...) \
    ARG_SIZE_FUNC_5(name, ret, __VA_ARGS__) \
    EXPORT(int64_t) GENERATED_##name##_arg5Size() { return FunctionTraits<ret, __VA_ARGS__>::argSizeAt<5>(); }

#define ARG_SIZE_FUNC_7(name, ret, ...) \
    ARG_SIZE_FUNC_6(name, ret, __VA_ARGS__) \
    EXPORT(int64_t) GENERATED_##name##_arg6Size() { return FunctionTraits<ret, __VA_ARGS__>::argSizeAt<6>(); }

#define ARG_SIZE_FUNC_8(name, ret, ...) \
    ARG_SIZE_FUNC_7(name, ret, __VA_ARGS__) \
    EXPORT(int64_t) GENERATED_##name##_arg7Size() { return FunctionTraits<ret, __VA_ARGS__>::argSizeAt<7>(); }

#define ARG_SIZE_FUNC_9(name, ret, ...) \
    ARG_SIZE_FUNC_8(name, ret, __VA_ARGS__) \
    EXPORT(int64_t) GENERATED_##name##_arg8Size() { return FunctionTraits<ret, __VA_ARGS__>::argSizeAt<8>(); }

#define ARG_SIZE_FUNC_10(name, ret, ...) \
    ARG_SIZE_FUNC_9(name, ret, __VA_ARGS__) \
    EXPORT(int64_t) GENERATED_##name##_arg9Size() { return FunctionTraits<ret, __VA_ARGS__>::argSizeAt<9>(); }

// Macro to select which set of functions to generate based on a count
#define ARG_SIZE_FUNC(name, ret, N, ...) \
    CONCATENATE(ARG_SIZE_FUNC_, N)(name, ret, __VA_ARGS__)

// End of the arguments size ============================================

//ARG_SIZE_FUNC(name, ret, VAR_COUNT(__VA_ARGS__), _INTERNAL_ARGS_TYPES(__VA_ARGS__))

#define MANAGED_EXPORT(ret, name, ...) \
\
EXPORT(int64_t) GENERATED_##name##_argsCount() { return FunctionTraits<ret, _INTERNAL_ARGS_TYPES(__VA_ARGS__)>::argsCount(); } \
\
EXPORT(int64_t) GENERATED_##name##_argsSize() { return FunctionTraits<ret, _INTERNAL_ARGS_TYPES(__VA_ARGS__)>::argsSize(); } \
\
EXPORT(int64_t) GENERATED_##name##_returnSize() { return FunctionTraits<ret, _INTERNAL_ARGS_TYPES(__VA_ARGS__)>::returnSize(); } \
\
ARG_SIZE_FUNC(name, ret, VAR_COUNT(_INTERNAL_ARGS_TYPES(__VA_ARGS__)), _INTERNAL_ARGS_TYPES(__VA_ARGS__)) \
\
EXPORT(ret) name(_INTERNAL_ARGS(__VA_ARGS__))
   
#endif // UNITYINTEROP_H