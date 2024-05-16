#pragma once

#ifndef CONCURRENTQUEUE_H
#define CONCURRENTQUEUE_H

#include <queue>
#include <mutex>

template<typename T>
class ThreadSafeQueue {
private:
    std::queue<T> queue;
    mutable std::mutex mutex;

public:
    ThreadSafeQueue() = default;
    ThreadSafeQueue(const ThreadSafeQueue<T>&) = delete; // Copy constructor
    ThreadSafeQueue& operator=(const ThreadSafeQueue<T>&) = delete; // Copy assignment

    void push(T value) {
        std::lock_guard<std::mutex> lock(mutex);
        queue.push(value);
    }

    bool try_pop(T& value) {
        std::lock_guard<std::mutex> lock(mutex);
        if (queue.empty()) {
            return false;
        }
        value = queue.front();
        queue.pop();
        return true;
    }

    bool empty() const {
        std::lock_guard<std::mutex> lock(mutex);
        return queue.empty();
    }
};

#endif // CONCURRENTQUEUE_H
