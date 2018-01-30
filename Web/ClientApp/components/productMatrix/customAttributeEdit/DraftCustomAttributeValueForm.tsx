import * as React from 'react';
import { Field, FieldArray, reduxForm, InjectedFormProps } from 'redux-form'
import { Table, Input, Button, FormFeedback, Form, Label, ButtonGroup } from 'reactstrap';
import * as classnames from 'classnames';
import { required, dateTime, integer, float, minValue, maxValue } from '../../../helpers/validation';
import InputFormField from '../../form/InputFormField';
import ValidatedInputFormField from '../../form/ValidatedInputFormField';
import ValueRadioButton from './form/radioButtons/ValueRadioButton';
import ValueIsNotSetRadioButton from './form/radioButtons/ValueIsNotSetRadioButton';
import ValueIsSetRadioButton from './form/radioButtons/ValueIsSetRadioButton';
import ValueIsIrrelevantRadioButton from './form/radioButtons/ValueIsIrrelevantRadioButton';
import ValueComponent from './form/valueComponents/ValueComponent';
import CurrentCopyToDraft from '../../../containers/products/customAttributeEdit/CurrentCopyToDraft';
import { CustomAttributeValueDetail } from '../../../models/CustomAttributeValue';
import * as CustomAttributeValueStore from '../../../store/CustomAttributeValue';
import * as moment from 'moment';
import config from '../../../configuration/config';
import SaveDraftCustomAttributeValueButton from './form/actionComponents/SaveDraftCustomAttributeValueButton';
import ApproveDraftCustomAttributeValueButton from './form/actionComponents/ApproveDraftCustomAttributeValueButton';
import UnapproveDraftCustomAttributeValueButton from './form/actionComponents/UnapproveDraftCustomAttributeValueButton';
import { ProductVariant } from '../../../apimodels/ProductVariantsStore';
import { CustomAttributeType } from '../../../apimodels/CustomAttributeValueStore';
import { CustomAttribute } from '../../../apimodels/ProductMatrixStore';
import * as customAttributePermissions from '../../../helpers/security/customAttributePermissions';
import PublishedFromConfirmDialog from './form/PublishedFromConfirmDialog';
import { CustomAttributeValueModelState, CustomAttributeValueModel } from '../../../models/CustomAttributeValue'
import { getCustomAttributeValueByType } from '../../../helpers/customAttributeValues'

export const FormName = "DraftCustomAttributeValueForm";

const DraftValueIsIrrelevant = "draftValueIsIrrelevant";
const DraftValueIsNotSet = "draftValueIsNotSet";
const DraftValueIsSet = "draftValueIsSet";
const Draft = "draft";

interface CellEditFormProps extends InjectedFormProps {
    customAttributeValue: CustomAttributeValueDetail;
    productVariant: ProductVariant;
    customAttribute: CustomAttribute;
    isDraftInEditMode: boolean;
}

interface CellEditFormState {
    showPublishedFromConfirmDialog: boolean;
    validFrom: string;
}

type InputElementEvent = React.ChangeEvent<HTMLInputElement> | React.MouseEvent<HTMLInputElement> | React.FocusEvent<HTMLInputElement>;

class DraftCustomAttributeValueForm extends React.Component<CellEditFormProps & typeof CustomAttributeValueStore.actionCreators & CustomAttributeValueStore.CustomAttributeValueState, CellEditFormState> {
    constructor(props) {
        super(props);

        this.state = {
            showPublishedFromConfirmDialog: false,
            validFrom: null
        }
    }

    onSubmit = (values) => {
        switch (this.props.draftEditFormAction) {
            case CustomAttributeValueStore.DraftEditFormAction.SaveDraft:
                this.props.postSaveDraft(FormName, this.props.customAttribute.id, this.props.productVariant.id);
                break;

            case CustomAttributeValueStore.DraftEditFormAction.ApproveDraft:
                if (values.draft.publishedFrom) {
                    this.props.postApproveDraft(FormName, this.props.customAttribute.id, this.props.productVariant.id);
                } else {
                    this.setState({ showPublishedFromConfirmDialog: true, validFrom: values.draft.validFrom });
                }
                break;

            case CustomAttributeValueStore.DraftEditFormAction.UnapproveDraft:
                this.props.postUnapproveDraft(FormName, this.props.customAttributeValue.draft.customAttributeValueId, this.props.customAttribute.id, this.props.productVariant.id);
                break;
            default:
        }
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

    confirmPublishedFromAndPostApproveDraft = () => {
        let validFrom = this.state.validFrom;

        if (this.state.validFrom && Date.parse(this.state.validFrom) < Date.now()) {
            validFrom = new Date().toString();
        }

        this.props.change("draft.publishedFrom", moment(validFrom).format(config.inputDateFormat));
        this.props.postApproveDraft(FormName, this.props.customAttribute.id, this.props.productVariant.id);
    }



    closePublishedFromConfirmDialog = () => this.setState({ showPublishedFromConfirmDialog: false })

    render() {

        const {
            handleSubmit,
            pristine,
            submitting,
            valid,
            change,
            customAttributeValue,
            reset,
            isDraftInEditMode
        } = this.props as CellEditFormProps & InjectedFormProps;

        const isFormReadyToSubmit = (!submitting || !pristine);

        const hasPermissionToEditDraft = customAttributePermissions.hasPermissionToEditDraft(customAttributeValue, isDraftInEditMode);
        const hasPermissionToSaveDraft = customAttributePermissions.hasPermissionToSaveDraft(customAttributeValue, isDraftInEditMode);
        const hasPermissionToApproveDraft = customAttributePermissions.hasPermissionToApproveDraft(customAttributeValue, isDraftInEditMode);
        const hasPermissionToUnapproveDraft = customAttributePermissions.hasPermissionToUnapproveDraft(customAttributeValue, isDraftInEditMode);

        return <form id={FormName} onSubmit={handleSubmit(this.onSubmit)} noValidate className={classnames({})} onReset={reset}>
            <div className="row mb-2">
                <div className="col-1">{hasPermissionToEditDraft && <CurrentCopyToDraft onCopyCurrentToDraft={(fieldName, fieldValue) => change(fieldName, fieldValue)} />}</div>
                <div className="col">
                    <b>Koncept (nová verze)</b>
                </div>
            </div>
            <div className="row mb-2">
                <div className="col offset-1">
                    <span className="mr-3">
                        <ValueIsSetRadioButton
                            id={DraftValueIsSet}
                            name={Draft}
                            disabled={!hasPermissionToEditDraft}
                            onChange={(event, elementId) => this.onValueChange(event, elementId)}
                        />
                    </span>

                    <ValueComponent
                        id={DraftValueIsSet}
                        name={Draft}
                        customAttribute={customAttributeValue.customAttribute}
                        customAttributeValue={customAttributeValue.draft}
                        onValueChange={(event, elementId) => this.onValueChange(event, elementId)}
                        disabled={!hasPermissionToEditDraft}
                        onSimulateValueClick={this.simulateValueClick} />
                </div>
            </div>
            <div className="row mb-2">
                <div className="col offset-1">
                    <ValueIsNotSetRadioButton
                        id={DraftValueIsNotSet}
                        name={Draft}
                        disabled={!hasPermissionToEditDraft}
                        onChange={(event, elementId) => this.onValueChange(event, elementId)}
                    />
                    &nbsp;
                    <Label htmlFor={DraftValueIsNotSet}>Nestanoveno</Label>
                </div>
            </div>
            <div className="row mb-2">
                <div className="col offset-1">
                    <ValueIsIrrelevantRadioButton
                        id={DraftValueIsIrrelevant}
                        name={Draft}
                        disabled={!hasPermissionToEditDraft}
                        onChange={(event, elementId) => this.onValueChange(event, elementId)}
                    />
                    &nbsp;
                    <Label htmlFor={DraftValueIsIrrelevant}>&times; (irelevantní)</Label>
                </div>
            </div>
            <div className="row mb-2">
                <div className="col-1"></div>
                <div className="col">
                    <Field type="textarea" name="draft.text" component={InputFormField} disabled={!hasPermissionToEditDraft} />
                </div>
            </div>
            {
                customAttributeValue.customAttribute.usesSatisfactionRate &&
                <div className="row mb-2">
                    <div className="col-1"></div>
                    <div className="col">
                        <Field
                            type="number"
                            min="0"
                            max="100"
                            name="draft.satisfactionRatePercent"
                            component={ValidatedInputFormField}
                            disabled={!hasPermissionToEditDraft}
                            validate={[required]}
                        />
                    </div>
                </div>
            }
            <div className="row mb-2">
                <div className="col-1"></div>
                <div className="col">
                    <Field type="textarea" name="draft.feedback" component={InputFormField} disabled={!hasPermissionToEditDraft} />
                </div>
            </div>
            <div className="row mb-2">
                <div className="col-1">&nbsp;</div>
                <div className="col">
                    <Field type="date" name="draft.validFrom" component={ValidatedInputFormField} disabled={!hasPermissionToEditDraft} validate={[required]} />
                </div>
            </div>
            <div className="row mb-2">
                <div className="col-1"></div>
                <div className="col">
                    <Field type="date" name="draft.publishedFrom" component={ValidatedInputFormField} disabled={!hasPermissionToEditDraft} />
                </div>
            </div>
            <div className="row mb-2">
                <div className="col-1"></div>
                <div className="col">
                    <SaveDraftCustomAttributeValueButton
                        canUserSubmit={isFormReadyToSubmit && hasPermissionToEditDraft}
                        onClick={e => this.props.setDraftEditFormAction(CustomAttributeValueStore.DraftEditFormAction.SaveDraft)} />
                </div>
            </div>
            <div className="row mb-2">
                <div className="col-1"></div>
                <div className="col">
                    <ApproveDraftCustomAttributeValueButton
                        canUserSubmit={isFormReadyToSubmit && hasPermissionToApproveDraft}
                        onClick={e => this.props.setDraftEditFormAction(CustomAttributeValueStore.DraftEditFormAction.ApproveDraft)} />
                </div>
            </div>
            <div className="row mb-2">
                <div className="col-1"></div>
                <div className="col">
                    <UnapproveDraftCustomAttributeValueButton
                        canUserSubmit={isFormReadyToSubmit && hasPermissionToUnapproveDraft}
                        onClick={e => this.props.setDraftEditFormAction(CustomAttributeValueStore.DraftEditFormAction.UnapproveDraft)} />
                </div>
            </div>
            <PublishedFromConfirmDialog closeDialog={this.closePublishedFromConfirmDialog} showDialog={this.state.showPublishedFromConfirmDialog} confirmPublishedFrom={() => this.confirmPublishedFromAndPostApproveDraft()} />
        </form>
    }
}

const validate = (values, props) => {
    const errors = { draft: {} };

    const draft = values.draft as CustomAttributeValueModel;
    const draftValue = getCustomAttributeValueByType(values.customAttribute.symbol as CustomAttributeType, values.draft as CustomAttributeValueModel);
    let errorMessage;

    if (draft.valueState === CustomAttributeValueModelState.IsSet) {
        errorMessage = required(draftValue.value);

        switch (values.customAttribute.symbol) {
            case CustomAttributeType.Boolean:
                break;

            case CustomAttributeType.BooleanExtended:
                break;

            case CustomAttributeType.Date:
                errorMessage = dateTime(draftValue.value);
                break;

            case CustomAttributeType.Decimal:
                errorMessage = float(draftValue.value);
                break;

            case CustomAttributeType.Integer:
                errorMessage = integer(draftValue.value);
                break;

            case CustomAttributeType.Text:
                break;

            default:
                throw new Error(`Unknown custom attribute type '${values.customAttribute.symbol}'.`);
        }

        if (errorMessage) {
            errors.draft[`${draftValue.key}`] = errorMessage;
        }
    }

    errorMessage = minValue(0)(draft.satisfactionRatePercent);
    if (errorMessage) {
        errors.draft["satisfactionRatePercent"] = errorMessage;
    }

    errorMessage = maxValue(100)(draft.satisfactionRatePercent);
    if (errorMessage) {
        errors.draft["satisfactionRatePercent"] = errorMessage;
    }

    return errors;
}

export default reduxForm({
    form: FormName,
    validate
})(DraftCustomAttributeValueForm)