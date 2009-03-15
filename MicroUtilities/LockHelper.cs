/* 
 * LockHelper.cs
 * 
 * Copyright (c) 2009, Elze Kool (http://www.microframework.nl)
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR
 * ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */

using System;
using System.Threading;
using System.Collections;

using Microsoft.SPOT;

namespace MFToolkit.MicroUtilities
{
    /// <summary>
    /// LockHelper can be used for easy Debugging of (dead)locks. 
    /// 
    /// This class is havely based on the work of Keving Moore:
    /// http://work.j832.com/2007/11/going-beyond-monitorenterexit-c-lock.html
    /// 
    /// I removed storing the calling thread and added a UniqueIdentifier.
    /// Is this identifier in you're code for lock blocks. In case of a deadlock you can easly find
    /// the block part of your code.
    /// </summary>
    public class LockHelper
    {
        // Object used for the actual locking
        private object _lockObject = new object();

        // Name of this lock object
        private readonly String _name;

        // Unique identifier of the object currently holding the lock
        private String _uniqueIdentifier = String.Empty;

        /// <summary>
        /// Unique identifier of the object currently holding the lock
        /// </summary>
        public String UniqueIdentifier
        {
            get
            {
                lock (_uniqueIdentifier)
                {
                    return _uniqueIdentifier;
                }
            }
        }

        /// <summary>
        /// Create a new LockHelper Instance
        /// </summary>
        /// <param name="Name">Name for LockHelper, Used for Debugging</param>
        public LockHelper(String Name)
        {
            if ((Name == null) || (Name == String.Empty))
                throw new NullReferenceException("Name");

            _name = Name;
        }

        /// <summary>
        /// Check if this LockHelper is locked. 
        /// </summary>
        /// <returns>True if locked</returns>
        public bool Locked()
        {
            lock (_uniqueIdentifier)
            {
                if (_uniqueIdentifier == String.Empty)
                    return false;
                else
                    return true;
            }
        }

        /// <summary>
        /// getLock is used in an using statement. When it's requested it enters a Lock Monitor.
        /// It returns an IDisposable Unlocker Object which is destroyed on the end of the using block.
        /// When it's destroyed it releases the Lock Monitor
        /// </summary>
        /// <param name="UniqueCallIdentifier">This parameter is stored and should hold an unique identifier so that it can be found in the source</param>
        /// <returns></returns>
        public Unlocker getLock(String UniqueCallIdentifier)
        {
            if ((UniqueCallIdentifier == null) || (UniqueCallIdentifier == String.Empty))
                throw new NullReferenceException("UniqueCallIdentifier");

            // Store Current Time
            long dt_before = DateTime.Now.Ticks;

            // Enter Lock
            Monitor.Enter(_lockObject);

            // Calculate the number of miliseconds it took to aquire the lock
            long TimeToLock = (DateTime.Now.Ticks - dt_before) / TimeSpan.TicksPerMillisecond;


            // Store Unique Call Identifier
            lock (_uniqueIdentifier)
            {
                _uniqueIdentifier = UniqueCallIdentifier;
            }

            // Debug message
            //Debug.Print("Lock " + _name + " aquired by " + UniqueCallIdentifier + " in " + TimeToLock.ToString() + " mSec");

            // Return new Unlocking Object
            return new Unlocker(this);
        }

        /// <summary>
        /// Internal Function, Should only be called by child Unlocker Object
        /// </summary>
        public void Unlock()
        {
            // Store Unique Call Identifier
            lock (_uniqueIdentifier)
            {
                // Debug message
                //Debug.Print("Released " + _name + " aquired by " + _uniqueIdentifier);

                _uniqueIdentifier = String.Empty;
            }

            // Release Lock
            Monitor.Exit(_lockObject);
        }

        public class Unlocker : IDisposable
        {
            // Used to store parent LockHelper Object
            private LockHelper _parent;

            // Object used to prevent multiple Dispose calls
            private object _lockObject = new object();

            public Unlocker(LockHelper Parent)
            {
                if (Parent == null)
                    throw new NullReferenceException("Parent");

                _parent = Parent;
            }

            public void Dispose()
            {
                lock (_lockObject)
                {
                    if (_parent != null)
                    {
                        _parent.Unlock();
                        _parent = null;
                    }
                    else
                    {
                        throw new Exception("Lock already released.");
                    }

                }
            }
        }

    }
}
