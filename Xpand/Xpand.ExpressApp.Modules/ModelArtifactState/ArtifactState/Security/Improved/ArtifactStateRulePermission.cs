﻿using DevExpress.Xpo;
using Xpand.ExpressApp.Logic.Security.Improved;
using Xpand.ExpressApp.ModelArtifactState.ArtifactState.Logic;

namespace Xpand.ExpressApp.ModelArtifactState.ArtifactState.Security.Improved {
    public abstract class ArtifactStateRulePermission : LogicRulePermission, IContextArtifactStateRule {
        protected ArtifactStateRulePermission(string operation, ArtifactStateOperationPermissionData logicRule)
            : base(operation, logicRule) {
            Module = logicRule.Module;
        }
        #region IArtifactRule Members
        [DisplayName("Module (regex)")]
        public string Module { get; set; }
        #endregion
    }
}
