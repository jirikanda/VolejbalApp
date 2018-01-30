import * as React from 'react';
import { Field, FieldArray, reduxForm, InjectedFormProps, ValidateCallback, WrappedFieldProps, formValueSelector, getFormValues, DecoratedComponentClass } from 'redux-form'
import { connect } from 'react-redux';
import { ApplicationState } from '../../../store/index';
import * as CustomAttributeValueStore from '../../../store/CustomAttributeValue';
import CurrentCustomAttributeValueForm, { FormName as CurrentCustomAttributeValueFormName } from '../../../components/productMatrix/customAttributeEdit/CurrentCustomAttributeValueForm'
import DraftCustomAttributeValueForm, { FormName as DraftCustomAttributeValueFormName } from '../../../components/productMatrix/customAttributeEdit/DraftCustomAttributeValueForm'
import CellEditReadOnlyInfoTable from '../../../components/productMatrix/customAttributeEdit/CellEditReadOnlyInfoTable';
import { ProductVariant } from '../../../apimodels/ProductVariantsStore';
import { CustomAttribute } from '../../../apimodels/ProductMatrixStore';
import { Topic } from '../../../apimodels/TopicsStore';

interface CellEditFormPropsPublic {
    productVariant: ProductVariant;
    customAttribute: CustomAttribute;
    ancestorTopicsAndMe: Topic[];
}

type CellEditFormProps =
    CellEditFormPropsPublic
    & CustomAttributeValueStore.CustomAttributeValueState
    & typeof CustomAttributeValueStore.actionCreators;

class CellEditForm extends React.Component<CellEditFormProps> {
    componentWillMount() {
        this.props.requestCustomAttributeValue(this.props.productVariant.id, this.props.customAttribute.id, [CurrentCustomAttributeValueFormName, DraftCustomAttributeValueFormName]);
    }

    componentWillReceiveProps(nextProps: CellEditFormPropsPublic) {
        if (this.props.customAttribute.id !== nextProps.customAttribute.id && this.props.productVariant.id !== nextProps.productVariant.id) {
            this.props.requestCustomAttributeValue(nextProps.productVariant.id, nextProps.customAttribute.id, [CurrentCustomAttributeValueFormName, DraftCustomAttributeValueFormName]);
        }
    }

    render() {
        return <>
            {this.props.customAttributeValue && <><CellEditReadOnlyInfoTable
                ancestorTopicsAndMe={this.props.ancestorTopicsAndMe}
                customAttribute={this.props.customAttributeValue.customAttribute}
                productVariant={this.props.productVariant} />
                <div className="row">
                    <div className="col-7">
                        <CurrentCustomAttributeValueForm {...this.props} />
                    </div>
                    <div className="col">
                        <DraftCustomAttributeValueForm {...this.props} />
                    </div>
                </div>
            </>
        }            
        </>
    }
}

export default connect((state: ApplicationState, props: CellEditFormPropsPublic) => state.customAttributeValue, CustomAttributeValueStore.actionCreators)(CellEditForm);