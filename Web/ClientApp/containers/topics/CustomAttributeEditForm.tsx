import * as React from 'react';
import { reduxForm, InjectedFormProps, Field, getFormValues } from 'redux-form';
import { Row, Col, Form, FormGroup, Label, Input, Button, Dropdown, DropdownToggle, DropdownMenu, DropdownItem } from 'reactstrap';
import ValidatedInputFormField from '../../components/form/ValidatedInputFormField';
import { required } from '../../helpers/validation';
import * as CustomAttributesStore from '../../store/CustomAttributes';
import * as ProductTypesStore from '../../store/ProductTypes';
import * as TopicsStore from '../../store/Topics';
import * as CodebooksStore from '../../store/Codebooks';
import { CustomAttributeType } from '../../apimodels/CustomAttributeValueStore';
import { CustomAttributeVM } from '../../apimodels/TopicsStore';
import { getCustomAttributeTypeText } from '../../helpers/customAttributeValues';
import { connect } from 'react-redux';

export const FormName = "CustomAttributeEditForm";

type CustomAttributeEditFormProps =
    CodebooksStore.CodebooksState
    & CustomAttributesStore.CustomAttributeState
    & TopicsStore.TopicState
    & ProductTypesStore.ProductTypesState
    & typeof CustomAttributesStore.actionCreators
    & typeof TopicsStore.actionCreators
    & InjectedFormProps
    & { canSetMeasureUnit: boolean, customAttributeMeasureUnitId: number }

class CustomAttributeEditForm extends React.Component<CustomAttributeEditFormProps> {
    onSubmit = () => {
        // TODO: technical debt: callback are pure evil within redux environment
        this.props.saveCustomAttribute(FormName, this.props.productType.id, this.props.topicVM.id, () => {
            this.props.requestTopics(this.props.productType.id);
            this.props.requestCustomAttribute(FormName, this.props.productType.id, this.props.customAttributeVM.id);
        });
    }

    deleteCustomAttributeClicked = () => {
        // TODO: technical debt: callback are pure evil within redux environment
        this.props.deleteCustomAttribute(this.props.productType.id, () => {
            this.props.requestTopics(this.props.productType.id);
        })
    }

    componentWillReceiveProps(nextProps: CustomAttributeEditFormProps) {
        const { change } = this.props;
        const { canSetMeasureUnit, customAttributeVM, measureUnits, customAttributeMeasureUnitId } = nextProps;

        this.props.change("measureUnitId", canSetMeasureUnit ? (customAttributeMeasureUnitId || measureUnits[0].id) : null);
    }

    render() {
        const { handleSubmit, customAttributeVM, measureUnits, canSetMeasureUnit } = this.props;

        const canDeleteCustomAttribute = customAttributeVM && customAttributeVM.topicId !== null;
        const canChangeCustomAttributeType = customAttributeVM && customAttributeVM.topicId === null;

        const customAttributeTypes = Object.keys(CustomAttributeType).filter(key => isNaN(parseInt(key))).map(key => CustomAttributeType[key] as CustomAttributeType);

        return <>
            <Row>
                <Col sm={2}>
                    <Button disabled={!canDeleteCustomAttribute} onClick={this.deleteCustomAttributeClicked}>Smazat upřesňující podmínku</Button>
                </Col>
            </Row>
            <form id={FormName} onSubmit={handleSubmit(this.onSubmit)}>
                <FormGroup row>
                    <Label for="name" sm={2}>Název*</Label>
                    <Col sm={10}>
                        <Field name="name" id="name" component={ValidatedInputFormField} validate={[required]} />
                    </Col>
                </FormGroup>
                <FormGroup row>
                    <Label for="type" sm={2}>Datový typ</Label>
                    <Col sm={10}>
                        <Field key="type" name="type" id="type" component="select" parse={value => parseInt(value)} format={value => value + ""} disabled={!canChangeCustomAttributeType}>
                            {customAttributeTypes.map(customAttributeType =>
                                <option key={customAttributeType} value={customAttributeType}>
                                    {getCustomAttributeTypeText(customAttributeType)}
                                </option>
                            )}
                        </Field>
                    </Col>
                </FormGroup>
            {canSetMeasureUnit &&
                <FormGroup row>
                    <Label for="measureUnitId" sm={2}>Měrná jednotka</Label>
                    <Col sm={10}>
                        <Field key="measureUnitId" name="measureUnitId" id="measureUnitId" component="select" parse={value => parseInt(value)} format={value => value + ""}>
                            {measureUnits.map(measureUnit =>
                                <option key={measureUnit.id} value={measureUnit.id}>
                                    {measureUnit.name}
                                </option>
                            )}
                        </Field>
                    </Col>
                    </FormGroup>
                }
                <FormGroup row>
                    <Label for="usesSatisfactionRate" sm={2}>Míra uspokojení</Label>
                    <Col sm={10}>
                        {
                            [true, false].map(value => {
                                return <Label key={`usesSatisfactionRate.${value}`} htmlFor={`usesSatisfactionRate.${value}`}>
                                    <Field key="usesSatisfactionRate" name="usesSatisfactionRate" id={`usesSatisfactionRate.${value}`}
                                        type="radio"
                                        component="input"
                                        value={value as any} // HACK: redux-form needs to get value as boolean as it is NO as string
                                        parse={value => value === "true"} />
                                    {value ? "Ano" : "Ne"}
                                </Label>
                            })
                        }
                    </Col>
                </FormGroup>
                <FormGroup row>
                    <Label for="includedInQuickSelection" sm={2}>Rychlý výběr</Label>
                    <Col sm={10}>
                        {
                            [true, false].map(value =>
                                <Label key={`includedInQuickSelection.${value}`} htmlFor={`includedInQuickSelection.${value}`}>
                                    <Field key="includedInQuickSelection" name="includedInQuickSelection" id={`includedInQuickSelection.${value}`}
                                        type="radio"
                                        component="input"
                                        value={value as any} // HACK: redux-form needs to get value as boolean as it is NO as string
                                        parse={value => value === "true"} />
                                    {value ? "Ano" : "Ne"}
                                </Label>
                            )
                        }
                    </Col>
                </FormGroup>
                <Button>Uložit</Button>
            </form>
            </>
    }
}

export default reduxForm({
    form: FormName
})(CustomAttributeEditForm)