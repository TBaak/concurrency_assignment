namespace readytogo;

internal class Cook
{
    // do you need to add variables here?
    // add the variables you need for concurrency here


    // do not add more variables after this comment.
    private readonly int id;

    public Cook(int id) // you can add more parameters if you need
    {
        this.id = id;
    }

    internal void DoWork() // do not change the signature of this method
        // this method is not working properly
    {
        // Start cook in new thread
        Thread cookThread = new Thread(() =>
        {
            // Wait for the order to be placed, i.e. wait for the semaphore to open a spot
            Program.OrdersSemaphore.WaitOne();
            
            Order? o = null;

            // Lock the orders so another thread wont change it between line 32 and 33
            lock (Program.OrdersLock)
            {
                // each cook will ONLY get a dish from ONE order and prepare it
                o = Program.orders.First(); // do not remove this line
                Program.orders.RemoveFirst(); // do not remove this line
            }

            Console.WriteLine("K: Order taken by {0}, now preparing", id); // do not remove this line

            Thread.Sleep(new Random().Next(100, 500)); // do not remove this line
            // preparing an order takes time

            // when the order is ready, it is placed in the pickup location by the cook that made it.

            o.Done(); // the order is now ready
            Console.WriteLine("K: Order is: {0}", o.isReady()); // do not remove this line

            // Lock the pickups zo the adding is atomic
            lock (Program.PickupsLock)
            {
                Program.pickups.AddFirst(o); // do not remove this line
            }
            
            // now the client can pickup the order
            Program.PickupsSemaphore.Release(1);

            Console.WriteLine("K: Order ready"); // do not remove this line
            // each cook will terminate after preparing one order
        });
        
        Program.CookThreads.Add(cookThread);
        cookThread.Start();
    }
}