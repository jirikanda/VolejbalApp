import * as React from 'react';
import { Field, FieldArray, reduxForm, InjectedFormProps, getFormValues } from 'redux-form'
import { Table, Input, Button, FormFeedback, Form, Label, ButtonGroup } from 'reactstrap';
import * as classnames from 'classnames';
import { required, dateTime, integer, float, minValue, maxValue } from '../../../helpers/validation';
import { getCustomAttributeValueByType } from '../../../helpers/customAttributeValues';
import InputFormField from '../../form/InputFormField';
import ValidatedInputFormField from '../../form/ValidatedInputFormField';
import ValueRadioButton from './form/radioButtons/ValueRadioButton';
import ValueIsNotSetRadioButton from './form/radioButtons/ValueIsNotSetRadioButton';
import ValueIsSetRadioButton from './form/radioButtons/ValueIsSetRadioButton';
import ValueIsIrrelevantRadioButton from './form/radioButtons/ValueIsIrrelevantRadioButton';
import ValueComponent from './form/valueComponents/ValueComponent';
import { CustomAttributeValueDetail } from '../../../models/CustomAttributeValue';
import * as CustomAttributeValueStore from '../../../store/CustomAttributeValue';
import * as moment from 'moment';
import config from '../../../configuration/config';
import UpdateCurrentCustomAttributeValueButton from './form/actionComponents/UpdateCurrentCustomAttributeValueButton';
import { ProductVariant } from '../../../apimodels/ProductVariantsStore';
import { CustomAttributeType } from '../../../apimodels/CustomAttributeValueStore';
import { CustomAttribute } from '../../../apimodels/ProductMatrixStore';
import * as customAttributePermissions from '../../../helpers/security/customAttributePermissions';
import { CustomAttributeValueModelState, CustomAttributeValueModel } from '../../../models/CustomAttributeValue'

export const FormName = "CurrentCustomAttributeValueForm";

const CurrentValueIsIrrelevant = "currentValueIsIrrelevant";
const CurrentValueIsNotSet = "currentValueIsNotSet";
const CurrentValueIsSet = "currentValueIsSet";
const Current = "current";

interface CellEditFormProps extends InjectedFormProps {
    customAttributeValue: CustomAttributeValueDetail;
    productVariant: ProductVariant;
    customAttribute: CustomAttribute;
}

type InputElementEvent = React.ChangeEvent<HTMLInputElement> | React.MouseEvent<HTMLInputElement> | React.FocusEvent<HTMLInputElement>;

class CurrentCustomAttributeValueForm extends React.Component<CellEditFormProps & typeof CustomAttributeValueStore.actionCreators> {

    onSubmit = (values) => {
        this.props.postUpdateCurrent(FormName, this.props.customAttribute.id, this.props.productVariant.id);
    };

    onValueChange = (event: InputElementEvent, elementId: string) => {
        const eventTarget = event.target as HTMLInputElement;

        if (['text', 'number', 'date'].some(type => eventTarget.type === type)) {
            this.simulateValueClick(elementId);
        }
    }

    simulateValueClick = (elementId: string) => {
        const element = document.querySelector(`#${elementId}`) as HTMLInputElement;
        element.click();
    }

    render() {

        const {
            handleSubmit,
            pristine,
            submitting,
            valid,
            change,
            customAttributeValue,
            reset
        } = this.props as CellEditFormProps & InjectedFormProps;

        const isFormReadyToSubmit = (!submitting || !pristine);

        const hasPermissionToUpdateCurrent = customAttributePermissions.hasPermissionToUpdateCurrent(customAttributeValue);

        return <form id={FormName} onSubmit={handleSubmit(this.onSubmit)} noValidate className={classnames({})} onReset={reset}>
            <div>
                <div className="row mb-2">
                    <div className="col offset-4">
                        <b>Aktuální hodnota (oprava)</b>
                    </div>
                </div>
                <div className="row mb-2">
                    <div className="col-4">Hodnota odpovědi*</div>
                    <div className="col-8">
                        <span className="mr-3"><ValueIsSetRadioButton
                            id={CurrentValueIsSet}
                            name={Current}
                            disabled={!hasPermissionToUpdateCurrent}
                            onChange={(event, elementId) => this.onValueChange(event, elementId)}
                        />
                        </span>
                        &nbsp;
                        <ValueComponent
                            id={CurrentValueIsSet}
                            name={Current}
                            customAttribute={customAttributeValue.customAttribute}
                            customAttributeValue={customAttributeValue.current}
                            onValueChange={(event, elementId) => this.onValueChange(event, elementId)}
                            disabled={!hasPermissionToUpdateCurrent}
                            onSimulateValueClick={this.simulateValueClick} />
                    </div>
                </div>
                <div className="row mb-2">
                    <div className="col offset-4">
                        <ValueIsNotSetRadioButton
                            id={CurrentValueIsNotSet}
                            name={Current}
                            disabled={!hasPermissionToUpdateCurrent}
                            onChange={(event, elementId) => this.onValueChange(event, elementId)}
                        />
                        &nbsp;
                        <Label htmlFor={CurrentValueIsNotSet}>Nestanoveno</Label>
                    </div>
                </div>
                <div className="row mb-2">
                    <div className="offset-4 col">
                        <ValueIsIrrelevantRadioButton
                            id={CurrentValueIsIrrelevant}
                            name={Current}
                            disabled={!hasPermissionToUpdateCurrent}
                            onChange={(event, elementId) => this.onValueChange(event, elementId)}
                        />
                        &nbsp;
                        <Label htmlFor={CurrentValueIsIrrelevant}>&times; (irelevantní)</Label>
                    </div>
                </div>
                <div className="row mb-2">
                    <div className="col-4 col-form-label">Textová reprezentace</div>
                    <div className="col">
                        <Field type="textarea" name="current.text" component={InputFormField} disabled={!hasPermissionToUpdateCurrent} />
                    </div>
                </div>
                {
                    customAttributeValue.customAttribute.usesSatisfactionRate &&
                    <div className="row mb-2">
                        <div className="col-4 col-form-label">Míra uspokojení*</div>
                        <div className="col">
                            <Field
                                type="number"
                                min="0"
                                max="100"
                                name="current.satisfactionRatePercent"
                                component={ValidatedInputFormField}
                            />
                        </div>
                    </div>
                }
                <div className="row mb-2">
                    <div className="col-4 col-form-label">Sdílená zkušenost</div>
                    <div className="col">
                        <Field type="textarea" name="current.feedback" component={InputFormField} disabled={!hasPermissionToUpdateCurrent} />
                    </div>
                </div>
                <div className="row mb-2">
                    <div className="col-4 col-form-label">Platnost od*</div>
                    <div className="col form-control-plaintext">{customAttributeValue.current && customAttributeValue.current.validFrom && moment(customAttributeValue.current.validFrom).format(config.dateFormat)}</div>
                </div>
                <div className="row mb-2">
                    <div className="col-4 col-form-label">Publikovat v metodikách od</div>
                    <div className="col form-control-plaintext">{customAttributeValue.current && customAttributeValue.current.publishedFrom && moment(customAttributeValue.current.publishedFrom).format(config.dateFormat)}</div>
                </div>
                <div className="row mb-2">
                    <div className="col-4 col-form-label">Schválil a publikoval</div>
                    <div className="col form-control-plaintext">{customAttributeValue.current && customAttributeValue.current.approved && `${moment(customAttributeValue.current.approved).format(config.dateFormat)} ${customAttributeValue.current.approvedBy}`}</div>
                </div>
                <div className="row mb-2">
                    <div className="col-4"></div>
                    <div className="col">
                        <UpdateCurrentCustomAttributeValueButton
                            canUserSubmit={isFormReadyToSubmit && hasPermissionToUpdateCurrent}
                            onClick={e => this.props.postUpdateCurrent(FormName, this.props.customAttribute.id, this.props.productVariant.id)}
                        />
                    </div>
                </div>
            </div >
        </form >
    }
}

const validate = (values, props) => {
    const errors = { current: {} };

    const current = values.current as CustomAttributeValueModel;
    const currentValue = getCustomAttributeValueByType(values.customAttribute.symbol as CustomAttributeType, values.current as CustomAttributeValueModel);
    let errorMessage;

    if (current.valueState === CustomAttributeValueModelState.IsSet) {
        errorMessage = required(currentValue.value);

        switch (values.customAttribute.symbol) {
            case CustomAttributeType.Boolean:
                break;

            case CustomAttributeType.BooleanExtended:
                break;

            case CustomAttributeType.Date:
                errorMessage = dateTime(currentValue.value);
                break;

            case CustomAttributeType.Decimal:
                errorMessage = float(currentValue.value);
                break;

            case CustomAttributeType.Integer:
                errorMessage = integer(currentValue.value);
                break;

            case CustomAttributeType.Text:
                break;

            default:
                throw new Error(`Unknown custom attribute type '${values.customAttribute.symbol}'.`);
        }

        if (errorMessage) {
            errors.current[`${currentValue.key}`] = errorMessage;
        }

        if (values.customAttribute.usesSatisfactionRate) {
            errorMessage = required(current.satisfactionRatePercent);
            if (errorMessage) {
                errors.current["satisfactionRatePercent"] = errorMessage;
            }

            errorMessage = minValue(0)(current.satisfactionRatePercent);
            if (errorMessage) {
                errors.current["satisfactionRatePercent"] = errorMessage;
            }

            errorMessage = maxValue(100)(current.satisfactionRatePercent);
            if (errorMessage) {
                errors.current["satisfactionRatePercent"] = errorMessage;
            }
        }
    }    

    return errors;
}

export default reduxForm({
    form: FormName,
    validate
})(CurrentCustomAttributeValueForm)