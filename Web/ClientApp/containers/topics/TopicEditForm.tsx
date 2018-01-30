import * as React from 'react';
import { reduxForm, InjectedFormProps, Field, submit } from 'redux-form';
import { Row, Col, Form, FormGroup, Label, Input, Button } from 'reactstrap';
import ValidatedInputFormField from '../../components/form/ValidatedInputFormField';
import { required, minLength } from '../../helpers/validation';
import * as TopicsStore from '../../store/Topics';
import * as ProductTypesStore from '../../store/ProductTypes';
import * as CustomAttributesStore from '../../store/CustomAttributes';
import { connect } from 'react-redux';
import { FormName as CustomAttributeFormName } from './CustomAttributeEditForm';

export const FormName = "TopicEditForm";

type TopicEditFormProps =
    TopicsStore.TopicState
    & ProductTypesStore.ProductTypesState
    & CustomAttributesStore.CustomAttributeState
    & typeof TopicsStore.actionCreators
    & typeof CustomAttributesStore.actionCreators
    & InjectedFormProps

class TopicEditForm extends React.Component<TopicEditFormProps> {
    onSubmit = () => {
        // TODO: technical debt: callback are pure evil within redux environment
        this.props.saveTopic(FormName, this.props.productType.id, () => {
            this.props.requestTopics(this.props.productType.id);
            this.props.requestTopic(FormName, this.props.productType.id, this.props.topicVM.id);
        });
    }

    insertTopicClicked = () => this.props.addSubtopic(FormName)
    insertCustomAttributeClicked = () => this.props.addCustomAttribute(CustomAttributeFormName);
    deleteTopicClicked = () => {
        this.props.deleteTopic(this.props.productType.id, () => {
            this.props.requestTopics(this.props.productType.id);
        })
    }

    render() {
        const { handleSubmit, topicVM } = this.props;

        const canInsertSubtopic = topicVM && topicVM.id !== null && !topicVM.hasCustomAttributes;
        const canInsertCustomAttribute = topicVM && topicVM.id !== null && !topicVM.hasChildren;
        const canDeleteSubtopic = topicVM && topicVM.id !== null;

        return <>
            <Row>
                <Col sm={2}>
                    <Button disabled={!canInsertSubtopic} onClick={this.insertTopicClicked}>Přidat podtéma</Button>
                </Col>
                <Col sm={2}>
                    <Button disabled={!canInsertCustomAttribute} onClick={this.insertCustomAttributeClicked}>Přidat podmínku</Button>
                </Col>
                <Col sm={2}>
                    <Button disabled={!canDeleteSubtopic} onClick={this.deleteTopicClicked}>Smazat téma</Button>
                </Col>
            </Row>
            <form id={FormName} onSubmit={handleSubmit(this.onSubmit)}>
                <FormGroup row>
                    <Label for="name" sm={2}>Název</Label>
                    <Col sm={10}>
                        <Field name="name" id="name" component={ValidatedInputFormField} />
                    </Col>
                </FormGroup>
                <FormGroup row>
                    <Label for="validFrom" sm={2}>Viditelné od</Label>
                    <Col sm={10}>
                        <Field name="validFrom" id="validFrom" type="date" component={ValidatedInputFormField} />
                    </Col>
                </FormGroup>
                <FormGroup row>
                    <Label for="validTo" sm={2}>Viditelné do</Label>
                    <Col sm={10}>
                        <Field name="validTo" id="validTo" type="date" component={ValidatedInputFormField} />
                    </Col>
                </FormGroup>
                <Button>Uložit</Button>
            </form>
            </>
    }
}

export default reduxForm({
    form: FormName
})(TopicEditForm)