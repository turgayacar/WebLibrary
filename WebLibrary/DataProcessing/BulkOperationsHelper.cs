using WebLibrary.Models;

namespace WebLibrary.DataProcessing
{
    /// <summary>
    /// Toplu işlemler için helper sınıfı
    /// </summary>
    public static class BulkOperationsHelper
    {
        /// <summary>
        /// Toplu veri ekleme işlemi
        /// </summary>
        /// <typeparam name="T">Veri tipi</typeparam>
        /// <param name="items">Eklenecek öğeler</param>
        /// <param name="batchSize">Batch boyutu</param>
        /// <param name="operation">Ekleme işlemi</param>
        /// <returns>ServiceResult</returns>
        public static async Task<ServiceResult<BulkOperationResult>> BulkInsertAsync<T>(
            IEnumerable<T> items,
            int batchSize,
            Func<IEnumerable<T>, Task<ServiceResult<bool>>> operation)
        {
            try
            {
                if (items == null || !items.Any())
                    return ServiceResult<BulkOperationResult>.Error("Eklenecek öğeler boş olamaz");

                if (batchSize <= 0)
                    return ServiceResult<BulkOperationResult>.Error("Batch boyutu 0'dan büyük olmalıdır");

                if (operation == null)
                    return ServiceResult<BulkOperationResult>.Error("Ekleme işlemi belirtilmedi");

                var result = new BulkOperationResult();
                var batches = items.Chunk(batchSize);

                foreach (var batch in batches)
                {
                    try
                    {
                        var batchResult = await operation(batch);
                        if (batchResult.IsSuccess)
                        {
                            result.SuccessCount += batch.Count();
                        }
                        else
                        {
                            result.FailedCount += batch.Count();
                            result.Errors.AddRange(batchResult.ErrorMessages);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount += batch.Count();
                        result.Errors.Add($"Batch işleme hatası: {ex.Message}");
                    }
                }

                result.TotalCount = items.Count();
                return ServiceResult<BulkOperationResult>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<BulkOperationResult>.Error($"Toplu ekleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Toplu veri güncelleme işlemi
        /// </summary>
        /// <typeparam name="T">Veri tipi</typeparam>
        /// <param name="items">Güncellenecek öğeler</param>
        /// <param name="batchSize">Batch boyutu</param>
        /// <param name="operation">Güncelleme işlemi</param>
        /// <returns>ServiceResult</returns>
        public static async Task<ServiceResult<BulkOperationResult>> BulkUpdateAsync<T>(
            IEnumerable<T> items,
            int batchSize,
            Func<IEnumerable<T>, Task<ServiceResult<bool>>> operation)
        {
            try
            {
                if (items == null || !items.Any())
                    return ServiceResult<BulkOperationResult>.Error("Güncellenecek öğeler boş olamaz");

                if (batchSize <= 0)
                    return ServiceResult<BulkOperationResult>.Error("Batch boyutu 0'dan büyük olmalıdır");

                if (operation == null)
                    return ServiceResult<BulkOperationResult>.Error("Güncelleme işlemi belirtilmedi");

                var result = new BulkOperationResult();
                var batches = items.Chunk(batchSize);

                foreach (var batch in batches)
                {
                    try
                    {
                        var batchResult = await operation(batch);
                        if (batchResult.IsSuccess)
                        {
                            result.SuccessCount += batch.Count();
                        }
                        else
                        {
                            result.FailedCount += batch.Count();
                            result.Errors.AddRange(batchResult.ErrorMessages);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount += batch.Count();
                        result.Errors.Add($"Batch işleme hatası: {ex.Message}");
                    }
                }

                result.TotalCount = items.Count();
                return ServiceResult<BulkOperationResult>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<BulkOperationResult>.Error($"Toplu güncelleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Toplu veri silme işlemi
        /// </summary>
        /// <typeparam name="T">Veri tipi</typeparam>
        /// <param name="items">Silinecek öğeler</param>
        /// <param name="batchSize">Batch boyutu</param>
        /// <param name="operation">Silme işlemi</param>
        /// <returns>ServiceResult</returns>
        public static async Task<ServiceResult<BulkOperationResult>> BulkDeleteAsync<T>(
            IEnumerable<T> items,
            int batchSize,
            Func<IEnumerable<T>, Task<ServiceResult<bool>>> operation)
        {
            try
            {
                if (items == null || !items.Any())
                    return ServiceResult<BulkOperationResult>.Error("Silinecek öğeler boş olamaz");

                if (batchSize <= 0)
                    return ServiceResult<BulkOperationResult>.Error("Batch boyutu 0'dan büyük olmalıdır");

                if (operation == null)
                    return ServiceResult<BulkOperationResult>.Error("Silme işlemi belirtilmedi");

                var result = new BulkOperationResult();
                var batches = items.Chunk(batchSize);

                foreach (var batch in batches)
                {
                    try
                    {
                        var batchResult = await operation(batch);
                        if (batchResult.IsSuccess)
                        {
                            result.SuccessCount += batch.Count();
                        }
                        else
                        {
                            result.FailedCount += batch.Count();
                            result.Errors.AddRange(batchResult.ErrorMessages);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount += batch.Count();
                        result.Errors.Add($"Batch işleme hatası: {ex.Message}");
                    }
                }

                result.TotalCount = items.Count();
                return ServiceResult<BulkOperationResult>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<BulkOperationResult>.Error($"Toplu silme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Toplu veri işleme (CRUD)
        /// </summary>
        /// <typeparam name="T">Veri tipi</typeparam>
        /// <param name="items">İşlenecek öğeler</param>
        /// <param name="batchSize">Batch boyutu</param>
        /// <param name="insertOperation">Ekleme işlemi</param>
        /// <param name="updateOperation">Güncelleme işlemi</param>
        /// <param name="deleteOperation">Silme işlemi</param>
        /// <param name="identifier">Öğe tanımlayıcısı</param>
        /// <returns>ServiceResult</returns>
        public static async Task<ServiceResult<BulkOperationResult>> BulkProcessAsync<T>(
            IEnumerable<T> items,
            int batchSize,
            Func<IEnumerable<T>, Task<ServiceResult<bool>>> insertOperation,
            Func<IEnumerable<T>, Task<ServiceResult<bool>>> updateOperation,
            Func<IEnumerable<T>, Task<ServiceResult<bool>>> deleteOperation,
            Func<T, object> identifier)
        {
            try
            {
                if (items == null || !items.Any())
                    return ServiceResult<BulkOperationResult>.Error("İşlenecek öğeler boş olamaz");

                if (batchSize <= 0)
                    return ServiceResult<BulkOperationResult>.Error("Batch boyutu 0'dan büyük olmalıdır");

                if (insertOperation == null || updateOperation == null || deleteOperation == null)
                    return ServiceResult<BulkOperationResult>.Error("İşlem fonksiyonları belirtilmedi");

                if (identifier == null)
                    return ServiceResult<BulkOperationResult>.Error("Tanımlayıcı fonksiyon belirtilmedi");

                var result = new BulkOperationResult();
                var batches = items.Chunk(batchSize);

                foreach (var batch in batches)
                {
                    try
                    {
                        // Burada gerçek uygulamada öğelerin durumuna göre işlem yapılır
                        // Örnek: Yeni, güncellenmiş, silinmiş olarak işaretlenmiş öğeler
                        var insertItems = batch.Where(item => true).ToList(); // Yeni öğe kontrolü
                        var updateItems = batch.Where(item => true).ToList(); // Güncellenmiş öğe kontrolü
                        var deleteItems = batch.Where(item => true).ToList(); // Silinmiş öğe kontrolü

                        // Ekleme işlemi
                        if (insertItems.Any())
                        {
                            var insertResult = await insertOperation(insertItems);
                            if (insertResult.IsSuccess)
                                result.SuccessCount += insertItems.Count;
                            else
                                result.FailedCount += insertItems.Count;
                        }

                        // Güncelleme işlemi
                        if (updateItems.Any())
                        {
                            var updateResult = await updateOperation(updateItems);
                            if (updateResult.IsSuccess)
                                result.SuccessCount += updateItems.Count;
                            else
                                result.FailedCount += updateItems.Count;
                        }

                        // Silme işlemi
                        if (deleteItems.Any())
                        {
                            var deleteResult = await deleteOperation(deleteItems);
                            if (deleteResult.IsSuccess)
                                result.SuccessCount += deleteItems.Count;
                            else
                                result.FailedCount += deleteItems.Count;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount += batch.Count();
                        result.Errors.Add($"Batch işleme hatası: {ex.Message}");
                    }
                }

                result.TotalCount = items.Count();
                return ServiceResult<BulkOperationResult>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<BulkOperationResult>.Error($"Toplu işleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Paralel toplu işlem
        /// </summary>
        /// <typeparam name="T">Veri tipi</typeparam>
        /// <param name="items">İşlenecek öğeler</param>
        /// <param name="batchSize">Batch boyutu</param>
        /// <param name="maxDegreeOfParallelism">Maksimum paralel işlem sayısı</param>
        /// <param name="operation">İşlem fonksiyonu</param>
        /// <returns>ServiceResult</returns>
        public static async Task<ServiceResult<BulkOperationResult>> BulkProcessParallelAsync<T>(
            IEnumerable<T> items,
            int batchSize,
            int maxDegreeOfParallelism,
            Func<IEnumerable<T>, Task<ServiceResult<bool>>> operation)
        {
            try
            {
                if (items == null || !items.Any())
                    return ServiceResult<BulkOperationResult>.Error("İşlenecek öğeler boş olamaz");

                if (batchSize <= 0)
                    return ServiceResult<BulkOperationResult>.Error("Batch boyutu 0'dan büyük olmalıdır");

                if (maxDegreeOfParallelism <= 0)
                    return ServiceResult<BulkOperationResult>.Error("Maksimum paralel işlem sayısı 0'dan büyük olmalıdır");

                if (operation == null)
                    return ServiceResult<BulkOperationResult>.Error("İşlem fonksiyonu belirtilmedi");

                var result = new BulkOperationResult();
                var batches = items.Chunk(batchSize).ToList();
                var semaphore = new SemaphoreSlim(maxDegreeOfParallelism, maxDegreeOfParallelism);
                var tasks = new List<Task>();

                foreach (var batch in batches)
                {
                    var task = ProcessBatchAsync(batch, operation, semaphore, result);
                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);

                result.TotalCount = items.Count();
                return ServiceResult<BulkOperationResult>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<BulkOperationResult>.Error($"Paralel toplu işleme hatası: {ex.Message}");
            }
        }

        private static async Task ProcessBatchAsync<T>(
            IEnumerable<T> batch,
            Func<IEnumerable<T>, Task<ServiceResult<bool>>> operation,
            SemaphoreSlim semaphore,
            BulkOperationResult result)
        {
            await semaphore.WaitAsync();
            try
            {
                var batchResult = await operation(batch);
                if (batchResult.IsSuccess)
                {
                    lock (result)
                    {
                        result.SuccessCount += batch.Count();
                    }
                }
                else
                {
                    lock (result)
                    {
                        result.FailedCount += batch.Count();
                        result.Errors.AddRange(batchResult.ErrorMessages);
                    }
                }
            }
            catch (Exception ex)
            {
                lock (result)
                {
                    result.FailedCount += batch.Count();
                    result.Errors.Add($"Batch işleme hatası: {ex.Message}");
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Toplu işlem sonucu
        /// </summary>
        public class BulkOperationResult
        {
            /// <summary>
            /// Toplam işlenen öğe sayısı
            /// </summary>
            public int TotalCount { get; set; }

            /// <summary>
            /// Başarılı işlenen öğe sayısı
            /// </summary>
            public int SuccessCount { get; set; }

            /// <summary>
            /// Başarısız işlenen öğe sayısı
            /// </summary>
            public int FailedCount { get; set; }

            /// <summary>
            /// Hata mesajları
            /// </summary>
            public List<string> Errors { get; set; } = new List<string>();

            /// <summary>
            /// Başarı oranı
            /// </summary>
            public double SuccessRate => TotalCount > 0 ? (double)SuccessCount / TotalCount * 100 : 0;

            /// <summary>
            /// Hata oranı
            /// </summary>
            public double ErrorRate => TotalCount > 0 ? (double)FailedCount / TotalCount * 100 : 0;
        }
    }
}
