using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace NashLabelWorkStation.Util
{
	public class DelegateCommand : ICommand
	{
		#region Fields

		private readonly Action _executeMethod = null;
		private readonly Func<bool> _canExecuteMethod = null;

		#endregion

		#region Constructors

		public DelegateCommand( Action executeMethod, Func<bool> canExecuteMethod = null )
		{
			if ( executeMethod == null )
			{
				throw new ArgumentNullException( nameof( executeMethod ) );
			}

			_executeMethod = executeMethod;
			_canExecuteMethod = canExecuteMethod;
		}

		#endregion

		#region ICommand Members

		public bool CanExecute( object parameter )
		{
			if ( _canExecuteMethod != null )
			{
				return _canExecuteMethod();
			}
			return true;
		}

		public event EventHandler CanExecuteChanged
		{
			add
			{
				CommandManager.RequerySuggested += value;
			}
			remove
			{
				CommandManager.RequerySuggested -= value;
			}
		}

		public void Execute( object parameter )
		{
			_executeMethod();
		}

		#endregion
	}

	public class DelegateCommand<T> : ICommand
	{
		#region Fields

		private readonly Action<T> _executeMethod = null;
		private readonly Func<bool> _canExecuteMethod = null;

		#endregion

		#region Constructors

		public DelegateCommand( Action<T> executeMethod, Func<bool> canExecuteMethod = null )
		{
			if ( executeMethod == null )
			{
				throw new ArgumentNullException( "executeMethod" );
			}

			_executeMethod = executeMethod;
			_canExecuteMethod = canExecuteMethod;
		}

		#endregion

		#region ICommand Members

		public bool CanExecute( object parameter = null )
		{
			if ( _canExecuteMethod != null )
			{
				return _canExecuteMethod();
			}
			return true;
		}

		public event EventHandler CanExecuteChanged
		{
			add
			{
				CommandManager.RequerySuggested += value;
			}
			remove
			{
				CommandManager.RequerySuggested -= value;
			}
		}

		public void Execute( T parameter )
		{
			_executeMethod( parameter );
		}

		public void Execute( object parameter )
		{
			Execute( (T)parameter );
		}

		#endregion
	}
}
