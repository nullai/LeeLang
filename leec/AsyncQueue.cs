using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace leec
{
	public sealed class AsyncQueue<TElement>
	{
		private readonly TaskCompletionSource<bool> _whenCompleted = new TaskCompletionSource<bool>();

		// Note: All of the below fields are accessed in parallel and may only be accessed
		// when protected by lock (SyncObject)
		private readonly Queue<TElement> _data = new Queue<TElement>();
		private Queue<TaskCompletionSource<TElement>> _waiters;
		private bool _completed;
		private bool _disallowEnqueue;

		/// <summary>
		/// The number of unconsumed elements in the queue.
		/// </summary>
		public int Count
		{
			get
			{
				return _data.Count;
			}
		}

		/// <summary>
		/// Adds an element to the tail of the queue.  This method will throw if the queue 
		/// is completed.
		/// </summary>
		/// <exception cref="InvalidOperationException">The queue is already completed.</exception>
		/// <param name="value">The value to add.</param>
		public void Enqueue(TElement value)
		{
			if (!EnqueueCore(value))
			{
				throw new InvalidOperationException($"Cannot call {nameof(Enqueue)} when the queue is already completed.");
			}
		}

		/// <summary>
		/// Tries to add an element to the tail of the queue.  This method will return false if the queue
		/// is completed.
		/// </summary>
		/// <param name="value">The value to add.</param>
		public bool TryEnqueue(TElement value)
		{
			return EnqueueCore(value);
		}

		private bool EnqueueCore(TElement value)
		{
			if (_disallowEnqueue)
			{
				throw new InvalidOperationException($"Cannot enqueue data after PromiseNotToEnqueue.");
			}

			TaskCompletionSource<TElement> waiter;
			if (_completed)
			{
				return false;
			}

			if (_waiters == null || _waiters.Count == 0)
			{
				_data.Enqueue(value);
				return true;
			}

			Debug.Assert(_data.Count == 0);
			waiter = _waiters.Dequeue();

			// Invoke SetResult on a separate task, as this invocation could cause the underlying task to executing,
			// which could be a long running operation that can potentially cause a deadlock if executed on the current thread.
			waiter.SetResult(value);

			return true;
		}

		/// <summary>
		/// Attempts to dequeue an existing item and return whether or not it was available.
		/// </summary>
		public bool TryDequeue(out TElement d)
		{
			if (_data.Count == 0)
			{
				d = default(TElement);
				return false;
			}

			d = _data.Dequeue();
			return true;
		}

		/// <summary>
		/// Gets a value indicating whether the queue has completed.
		/// </summary>
		public bool IsCompleted
		{
			get
			{
				return _completed;
			}
		}

		/// <summary>
		/// Signals that no further elements will be enqueued.  All outstanding and future
		/// Dequeue Task will be cancelled.
		/// </summary>
		/// <exception cref="InvalidOperationException">The queue is already completed.</exception>
		public void Complete()
		{
			if (!CompleteCore())
			{
				throw new InvalidOperationException($"Cannot call {nameof(Complete)} when the queue is already completed.");
			}
		}

		public void PromiseNotToEnqueue()
		{
			_disallowEnqueue = true;
		}

		/// <summary>
		/// Same operation as <see cref="AsyncQueue{TElement}.Complete"/> except it will not
		/// throw if the queue is already completed.
		/// </summary>
		/// <returns>Whether or not the operation succeeded.</returns>
		public bool TryComplete()
		{
			return CompleteCore();
		}

		private bool CompleteCore()
		{
			Queue<TaskCompletionSource<TElement>> existingWaiters;
			if (_completed)
			{
				return false;
			}

			_completed = true;

			existingWaiters = _waiters;
			_waiters = null;

			if (existingWaiters?.Count > 0)
			{
				// cancel waiters.
				// NOTE: AsyncQueue has an invariant that 
				//       the queue can either have waiters or items, not both
				//       adding an item would "unwait" the waiters
				//       the fact that we _had_ waiters at the time we completed the queue
				//       guarantees that there is no items in the queue now or in the future, 
				//       so it is safe to cancel waiters with no loss of diagnostics
				Debug.Assert(this.Count == 0, "we should not be cancelling the waiters when we have items in the queue");
				foreach (var tcs in existingWaiters)
				{
					tcs.SetCanceled();
				}
			}

			_whenCompleted.SetResult(true);

			return true;
		}
	}
}
