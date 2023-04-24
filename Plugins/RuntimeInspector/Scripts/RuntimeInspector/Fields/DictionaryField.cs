using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

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

		protected override void GenerateElements()
		{
			if( Value == null )
				return;

			IDictionary dict = (IDictionary) Value;
			foreach(DictionaryEntry k_v in dict)
            {
				InspectorField keyDrawer = Inspector.CreateDrawerForType(keyType, drawArea, Depth + 1);
				if(keyDrawer == null)
                {
					break;
                }

				InspectorField valDrawer = Inspector.CreateDrawerForType(valType, drawArea, Depth + 1);
				if(valDrawer == null)
                {
					break;
                }

				keyDrawer.BindTo(keyType, "key", () => k_v.Key, value =>
				{
					//不修改Key
				});

				valDrawer.BindTo(valType, "val", () => k_v.Value, value =>
			    {
					IDictionary _dict = (IDictionary)Value;
					_dict[k_v.Key] = value;
					Value = _dict;
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

		private bool OnSizeChanged( BoundInputField source, string input )
		{			
			return false;
		}

		private object GetTemplateElement( object value )
		{
			return null;
		}
	}
}