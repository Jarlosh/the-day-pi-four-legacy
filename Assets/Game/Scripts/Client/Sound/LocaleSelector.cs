using Game.Client.UI;
using Game.Core;
using UnityEngine;

namespace Game.Client
{
	public class LocaleSelector: MonoBehaviour
	{
		private ILocaleSelectorService _selectorService; 
		private void Start()
		{
			_selectorService = ServiceLocator.Get<ILocaleSelectorService>();
		}
		
		public void ChangeLocale(int localeId)
		{
			_selectorService.ChangeLocale(localeId);
		}
	}
}