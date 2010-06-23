﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model.Core;
using eXpand.ExpressApp.Core.DynamicModel;
using DevExpress.ExpressApp.Model;

namespace eXpand.ExpressApp.SystemModule
{
    public abstract class GridOptionsController<ControlType, GridViewOptionsInterfaceType, TModelListViewMainViewOptionsInterfaceType, TListEditor> : ListViewController<TListEditor>, IModelExtender where TModelListViewMainViewOptionsInterfaceType : IModelNode where TListEditor : ListEditor {
        object _control;

        void IModelExtender.ExtendModelInterfaces(ModelInterfaceExtenders extenders) {
            extenders.Add<IModelListView,TModelListViewMainViewOptionsInterfaceType>();
            var modelApplicationCreatorProperties = ModuleBase.ModelApplicationCreatorProperties;
            ModelAutoGeneratedTypeCollector collector = modelApplicationCreatorProperties.AutoGeneratedTypeCollector;
            IEnumerable<DynamicModelType> dynamicModelTypes = GetDynamicModelTypes().ToList();
            foreach (var dynamicModelType in dynamicModelTypes) {
                collector.RegisterType(new ModelAutoGeneratedType(collector, dynamicModelType));
                extenders.Add(dynamicModelType.BaseType, dynamicModelType);
            }   
        }

        IEnumerable<DynamicModelType> GetDynamicModelTypes() {
            
            IEnumerable<PropertyInfo> propertyInfos = typeof(ControlType).GetProperties().Where(ControlPropertiesFilterPredicate());
            return from info in propertyInfos
                   let info2 = info
                   let firstOrDefault = typeof(GridViewOptionsInterfaceType).GetProperties().FirstOrDefault(info1 => info1.Name == info2.Name)
                   where firstOrDefault != null
                   select new DynamicModelType(firstOrDefault.PropertyType, info.PropertyType, null, DynamicPropertiesFilterPredicate());
        }

        protected abstract Func<PropertyInfo, bool> ControlPropertiesFilterPredicate();

        

        protected abstract Func<PropertyInfo, bool> DynamicPropertiesFilterPredicate();

        


        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            _control = GetControl();
            if (_control!= null) {
                var modelListViewMainViewOptionsInterfaceType = ((TModelListViewMainViewOptionsInterfaceType)View.Model);
                var propertyInfos = typeof(ControlType).GetProperties().Where(ControlPropertiesFilterPredicate());
                MethodInfo getValueMethodInfo = typeof(IModelNode).GetMethod("GetValue");
                IModelNode optionsNode = GetOptionNode(modelListViewMainViewOptionsInterfaceType);
                DelegateValuesFromModelToControl(optionsNode, propertyInfos, getValueMethodInfo);
            }            
        }

        void DelegateValuesFromModelToControl(IModelNode optionsNode, IEnumerable<PropertyInfo> propertyInfos, MethodInfo getValueMethodInfo) {            
            for (int i = 0; i < optionsNode.NodeCount; i++) {
                var modelNode = optionsNode.GetNode(i);
                var id = modelNode.GetValue<string>("Id");
                PropertyInfo propertyInfo = propertyInfos.FirstOrDefault(info => info.Name == id);
                if (propertyInfo != null){
                    object value = propertyInfo.GetValue(_control, null);
                    var properties = propertyInfo.PropertyType.GetProperties().Where(info => info.GetSetMethod() != null);
                    foreach (PropertyInfo property in properties) {
                        PropertyInfo info = modelNode.GetType().GetProperty(property.Name);
                        if (info!= null) {
                            MethodInfo genericMethod = getValueMethodInfo.MakeGenericMethod(info.PropertyType);
                            object invoke = genericMethod.Invoke(modelNode, new object[] {property.Name});
                            if (invoke!= null)
                                property.SetValue(value, invoke, null);
                        }
                    }
                }    
            }
        }

        IModelNode GetOptionNode(TModelListViewMainViewOptionsInterfaceType modelListViewMainViewOptionsInterfaceType) {
            IModelNode modelNode;
            for (int i = 0; i < modelListViewMainViewOptionsInterfaceType.NodeCount; i++) {
                modelNode = modelListViewMainViewOptionsInterfaceType.GetNode(i);
                var id = modelNode.GetValue<string>("Id");
                if (id == typeof(TModelListViewMainViewOptionsInterfaceType).GetProperties()[0].Name){
                    return modelNode;
                }
            }
            return null;
        }

        protected virtual object GetControl()
        {
            return View.Editor.Control;
        }        
    }
}