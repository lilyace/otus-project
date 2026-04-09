using System.Text;

namespace ParsingData.Tests
{
    public class SimpleStoreTest
    {
        [Fact]
        public async Task Store_Using_Correct()
        {
            //Arrange
            var writeTasks = new Task[4];
            var store = new SimpleStore();
            var writeValues = new[] {
                new UserProfile
                {
                    Id = 1,
                    CreatedAt = DateTime.Now,
                    Username = "Anton"
                },
                new UserProfile
                {
                    Id = 2,
                    CreatedAt = DateTime.Now,
                    Username = "Alex",
                },
                new UserProfile
                {
                    Id = 3,
                    CreatedAt = DateTime.Now,
                    Username = "Pavel"
                },
                new UserProfile
                {
                    Id = 4,
                    CreatedAt = DateTime.Now,
                    Username = "Dima"
                }};
            var readValues = new[] {
                new UserProfile
                {
                    Id = 3,
                    CreatedAt = DateTime.Now,
                    Username = "Pavel"
                },
                new UserProfile
                {
                    Id = 5,
                    CreatedAt = DateTime.Now,
                    Username = "Ivan"
                },
                new UserProfile
                {
                    Id = 1,
                    CreatedAt = DateTime.Now,
                    Username = "Anton"
                },
                new UserProfile
                {
                    Id = 3,
                    CreatedAt = DateTime.Now,
                    Username = "Pavel"
                } };
            var results = new UserProfile[4];

            //Act
            //на мой взгляд, проверить корректность данных в хранилище при многопоточной работе Get и Set несколько сложновато
            //т.к. примитив синхронизации гарантирует какой-либо доступ к ресурсу, но не определенный порядок выполнения действий
            //поэтому было решено запускать читателей в одном таске с паузами, чтобы можно было хоть как-то проверить результаты
            var readTask = Task.Run(async () => {
                results[0] = store.Get(readValues[0].Id.ToString());
                //имитация долгой работы, чтобы пока читатель думает, писатель что-то успел записать
                await Task.Delay(2000);

                results[1] = store.Get(readValues[1].Id.ToString());
                await Task.Delay(2000);

                results[2] = store.Get(readValues[2].Id.ToString());
                await Task.Delay(2000);

                results[3] = store.Get(readValues[3].Id.ToString());
                await Task.Delay(2000);
            });
            for (int i=0; i< 4; i++)
            {
                var local = i;
                writeTasks[local] = Task.Run(() => {
                    store.Set(writeValues[local].Id.ToString(), writeValues[local]);                                    
                });
            }

            await Task.WhenAll(writeTasks.Concat(new[] { readTask }).ToArray());
            var stats = store.GetStatistics();

            //Assert
            Assert.Equal(0, stats.DeleteCount);
            Assert.Equal(4, stats.GetCount);
            Assert.Equal(4, stats.SetCount);
            Assert.Null(results[0]);
            Assert.Equal(readValues[3], results[3]);

        }
    }
}
