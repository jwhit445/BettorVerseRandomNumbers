using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Async;
using DiscordRandomNumbers.Models;

namespace DiscordRandomNumbers.Storage {
	public abstract class StoreBase<T>  {

		protected abstract IEqualityComparer<T> Comparer { get; }

		protected HashSet<T> _listStoredObjects;
		protected AsyncLock _asyncLock = new();

		public StoreBase() {
		}

		public virtual async Task FillStore(List<T> listObjects) {
			if(listObjects == null) {
				throw new ArgumentNullException(nameof(listObjects));
			}

			await _asyncLock.Lock(() => {
				_listStoredObjects = new HashSet<T>(listObjects, Comparer);
			});
		}

		public async Task AddRange(List<T> listObjects) {
			if (listObjects == null) {
				throw new ArgumentNullException(nameof(listObjects));
			}

			await _asyncLock.Lock(() => {
				_listStoredObjects ??= new HashSet<T>(Comparer);
				foreach (var newItem in listObjects) {
					if (!_listStoredObjects.Contains(newItem)) {
						_listStoredObjects.Add(newItem);
					}
				}
			});
		}

		public async Task AddOne(T newItem) {
			_ = newItem ?? throw new ArgumentNullException(nameof(newItem));
			await _asyncLock.Lock(() => {
				_listStoredObjects ??= new HashSet<T>(Comparer);
				if (!_listStoredObjects.Contains(newItem)) {
					_listStoredObjects.Add(newItem);
				}
			});
		}

		public async Task RemoveOne(T oldItem) {
			_ = oldItem ?? throw new ArgumentNullException(nameof(oldItem));
			await _asyncLock.Lock(() => {
				_listStoredObjects ??= new HashSet<T>(Comparer);
				_listStoredObjects.Remove(oldItem);
			});
		}

		public async Task<List<T>> GetAll() {
			List<T> retVal = null;
			await _asyncLock.Lock(() => {
				_listStoredObjects ??= new HashSet<T>(Comparer);
				//Deep copy.
				retVal = new List<T>(_listStoredObjects);
			});
			return retVal;
		}

	}
}
