﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using SterlingExample.Database;
using Wintellect.Sterling;
using Wintellect.Sterling.IsolatedStorage;

namespace SterlingExample
{
    /// <summary>
    ///     Sterling application service
    /// </summary>
    public sealed class SterlingService : IApplicationService, IApplicationLifetimeAware, IDisposable
    {
        public const long KILOBYTE = 1024;
        public const long MEGABYTE = 1024*KILOBYTE;
        public const long QUOTA = 100*MEGABYTE;

        private SterlingEngine _engine;
        private static readonly ISterlingDriver _driver = new IsolatedStorageDriver(); // could use this: new MemoryDriver(); 


        private MainPage _mainPage;

        public UserControl MainVisual
        {
            get { return _mainPage.MainContent.Content as UserControl; }
            set { _mainPage.MainContent.Content = value;  }
        }

        public static SterlingService Current { get; private set; }

        public static void RestoreDatabase(BinaryReader br)
        {
            Current._engine.SterlingDatabase.Restore<FoodDatabase>(br);
        }

        public static void ShutDownDatabase()
        {
            if (Debugger.IsAttached)
            {
                Current._logger.Detach();
            }
            
            Current._engine.Dispose();                       
        }

        public static void StartUpDatabase()
        {
            if (Debugger.IsAttached)
            {
                Current._logger = new SterlingDefaultLogger(SterlingLogLevel.Verbose);
            }

            Current._engine = new SterlingEngine();
            Current._engine.Activate();
            Current.Database = Current._engine.SterlingDatabase.RegisterDatabase<FoodDatabase>(_driver);
        }

        /// <summary>
        ///     Navigator
        /// </summary>
        public Navigation Navigator { get; private set; }

        public ISterlingDatabaseInstance Database { get; private set; }
        
        private SterlingDefaultLogger _logger; 

        /// <summary>
        /// Called by an application in order to initialize the application extension service.
        /// </summary>
        /// <param name="context">Provides information about the application state. </param>
        public void StartService(ApplicationServiceContext context)
        {
            if (DesignerProperties.IsInDesignTool) return;
            _engine = new SterlingEngine();            
            Current = this;
        }

        /// <summary>
        /// Called by an application in order to stop the application extension service. 
        /// </summary>
        public void StopService()
        {
            return;
        }

        /// <summary>
        /// Called by an application immediately before the <see cref="E:System.Windows.Application.Startup"/> event occurs.
        /// </summary>
        public void Starting()
        {
            if (DesignerProperties.IsInDesignTool) return;
            
            if (Debugger.IsAttached)
            {
                _logger = new SterlingDefaultLogger(SterlingLogLevel.Verbose);
            }
           
            _engine.Activate();
            Database = _engine.SterlingDatabase.RegisterDatabase<FoodDatabase>(_driver);

            _mainPage = new MainPage();
            Application.Current.RootVisual = _mainPage;
            Navigator = new Navigation(view=>MainVisual=view);
        }

        public static void ExecuteOnUIThread(Action action)
        {
            if (Deployment.Current.CheckAccess())
            {
                var dispatcher = Deployment.Current.Dispatcher;
                if (dispatcher.CheckAccess())
                {
                    dispatcher.BeginInvoke(action);
                }
            }
            else
            {
                action();
            }
        }

        /// <summary>
        /// Called by an application immediately after the <see cref="E:System.Windows.Application.Startup"/> event occurs.
        /// </summary>
        public void Started()
        {
            return;
        }

        /// <summary>
        /// Called by an application immediately before the <see cref="E:System.Windows.Application.Exit"/> event occurs. 
        /// </summary>
        public void Exiting()
        {
            if (DesignerProperties.IsInDesignTool) return;
            
            if (Debugger.IsAttached && _logger != null)
            {
                _logger.Detach();
            }
        }

        /// <summary>
        /// Called by an application immediately after the <see cref="E:System.Windows.Application.Exit"/> event occurs. 
        /// </summary>
        public void Exited()
        {
            Dispose();
            _engine = null;
            return;
        }

        public void RequestRebuild()
        {
            if (RebuildRequested != null)
            {
                RebuildRequested(this, EventArgs.Empty);
            }
        }

        public event EventHandler RebuildRequested;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_engine != null)
            {
                _engine.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}
