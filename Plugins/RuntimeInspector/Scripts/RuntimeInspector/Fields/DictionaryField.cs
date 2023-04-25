using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using System.Linq;

namespace RuntimeInspectorNamespace
{
	public class DictionaryField : ExpandableInspectorField, IDropHandler
	{
#pragma warning disable 0649
		[SerializeField]
		private LayoutElement sizeLayoutElement;

		[SerializeField]
		private Text sizeText;

		[SerializeField]
		private BoundInputField sizeInput;
#pragma warning restore 0649

		private Type keyType;
		private Type valType;

		private readonly List<bool> elementsExpandedStates = new List<bool>();

		protected override int Length
		{
			get
			{
				IDictionary dict = (IDictionary) Value;
				if(dict != null )
                {
					return dict.Count;
				}
				else
                {
					return 0;
                }					
			}
		}

		public override void Initialize()
		{
			base.Initialize();

			sizeInput.Initialize();
			sizeInput.OnValueChanged += OnSizeInputBeingChanged;
			sizeInput.OnValueSubmitted += OnSizeChanged;
			sizeInput.DefaultEmptyValue = "0";
			sizeInput.CacheTextOnValueChange = false;
		}

		public override bool SupportsType( Type type )
		{
#if UNITY_EDITOR || !NETFX_CORE
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof( Dictionary<,> );
#else
			return type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof( Dictionary<,> );
#endif
		}

		protected override void OnBound( MemberInfo variable )
		{
			base.OnBound( variable );
			keyType = BoundVariableType.GetGenericArguments()[0];
			valType = BoundVariableType.GetGenericArguments()[1];
		}

		protected override void OnUnbound()
		{
			base.OnUnbound();

			sizeInput.Text = "0";
			elementsExpandedStates.Clear();
		}

		protected override void OnSkinChanged()
		{
			base.OnSkinChanged();

			sizeInput.Skin = Skin;

			sizeLayoutElement.SetHeight( Skin.LineHeight );
			sizeText.SetSkinText( Skin );

			Vector2 rightSideAnchorMin = new Vector2( Skin.LabelWidthPercentage, 0f );
			variableNameMask.rectTransform.anchorMin = rightSideAnchorMin;
			( (RectTransform) sizeInput.transform ).anchorMin = rightSideAnchorMin;
		}

		protected override void OnDepthChanged()
		{
			base.OnDepthChanged();
			sizeText.rectTransform.sizeDelta = new Vector2( -Skin.IndentAmount * ( Depth + 1 ), 0f );
		}

		protected override void ClearElements()
		{
			elementsExpandedStates.Clear();
			for( int i = 0; i < elements.Count; i++ )
				elementsExpandedStates.Add( ( elements[i] is ExpandableInspectorField ) ? ( (ExpandableInspectorField) elements[i] ).IsExpanded : false );

			base.ClearElements();
		}

		readonly static string cFakeFake = "_Fake_Fake_";

		protected override void GenerateElements()
		{
			if( Value == null )
				return;

			IDictionary dict = (IDictionary) Value;
			for(int i_key=0;i_key<dict.Keys.Count;i_key++)
            {
				InspectorField keyDrawer = Inspector.CreateDrawerForType(keyType, drawArea, Depth + 1);
				if(keyDrawer == null)
                {
					break;
                }

				StringField key_str_field = keyDrawer as StringField;
				if(key_str_field!=null)
                {
					key_str_field.SetterMode = StringField.Mode.OnSubmit;
                }

				InspectorField valDrawer = Inspector.CreateDrawerForType(valType, drawArea, Depth + 1);
				if(valDrawer == null)
                {
					break;
                }
				StringField val_str_field = valDrawer as StringField;
				if (val_str_field != null)
				{
					val_str_field.SetterMode = StringField.Mode.OnSubmit;
				}

				int saved_key_idx = i_key;

				keyDrawer.BindTo(keyType, "key", 
				() =>
				{
					var key_list = dict.Keys as IEnumerable<string>;
					if(key_list != null)
                    {
						return key_list.ElementAt(saved_key_idx);
                    }
					else
                    {
						return null;
                    }
				},
				value =>
				{
					var key_list = dict.Keys as IEnumerable<string>;
					if (key_list != null)
					{
						string old_key = key_list.ElementAt(saved_key_idx);
						string new_key = value as string;

						if (old_key != null && new_key != null)
						{
							if (old_key.Contains(cFakeFake))
							{
								var old_val = dict[old_key];
								dict[new_key] = old_val;
								dict.Remove(old_key);
								Value = dict;
							}
						}
					}
				});

				valDrawer.BindTo(valType, "val", 
				() =>
				{
					var key_list = dict.Keys as IEnumerable<string>;
					if (key_list != null)
					{
						var key = key_list.ElementAt(saved_key_idx);
						return dict[key];
					}
					else
					{
						return null;
					}
				},
				value =>
			    {
					var key_list = dict.Keys as IEnumerable<string>;
					if (key_list != null)
					{
						var key = key_list.ElementAt(saved_key_idx);
						dict[key] = value;
						Value = dict;
					}
			    });

				elements.Add(keyDrawer);
				elements1.Add(valDrawer);
			}

			sizeInput.Text = Length.ToString( RuntimeInspectorUtils.numberFormat );
			elementsExpandedStates.Clear();
		}

		void IDropHandler.OnDrop( PointerEventData eventData )
		{

		}

		private bool OnSizeInputBeingChanged( BoundInputField source, string input )
		{
			int value;
			if( int.TryParse( input, NumberStyles.Integer, RuntimeInspectorUtils.numberFormat, out value ) && value >= 0 )
				return true;

			return false;
		}

		private bool OnSizeChanged(BoundInputField source, string input)
		{
			int value;
			if (int.TryParse(input, NumberStyles.Integer, RuntimeInspectorUtils.numberFormat, out value) && value >= 0)
			{
				int delta_len = value - Length;
				if (delta_len > 0)
				{
					IDictionary dict = (IDictionary)Value;
					DictionaryEntry last_k_v;
					foreach (DictionaryEntry k_v in dict)
					{
						last_k_v = k_v;
					}
					for (int i = 0; i < delta_len; i++)
					{
						string str_key = last_k_v.Key as string;
						if (str_key != null)
						{
							string new_str_key = str_key + "_Fake_Fake_" + i.ToString();
							dict[new_str_key] = last_k_v.Value;
						}
					}
				}
				else
				{
					return false;
				}
			}
			else
            {
				return false;
            }
			return true;
		}
	}
}