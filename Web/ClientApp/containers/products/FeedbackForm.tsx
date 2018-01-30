import * as React from 'react';
import { connect } from 'react-redux';
import { Button } from 'reactstrap';
import Modal from '../../components/common/Modal';
import { Topic } from '../../apimodels/TopicsStore';
import { CustomAttributeValue, CustomAttribute, ProductVariant } from '../../apimodels/ProductMatrixStore';
import * as ProductMatrixStore from '../../store/ProductMatrix';
import { ApplicationState } from '../../store';
import TopicPath from '../../components/common/TopicPath';
import getCustomAttributeAnswer from '../../helpers/customAttributeAnswer';
import { Toastr } from '../../store/commonActionCreators/Toaster';
import { Field, FieldArray, reduxForm, InjectedFormProps } from 'redux-form';
import { required } from '../../helpers/validation';
import ValidatedInputFormField from '../../components/form/ValidatedInputFormField';

export const FormName = "FeedbackForm";

interface FeedbackFormProps extends InjectedFormProps {
    sendFeedback: (productVariant: ProductVariant, customAttribute: CustomAttribute, feedback: string) => void;
    closeFeedbackModalDialog: () => void;
    productVariant: ProductVariant;
    customAttribute: CustomAttribute;
}

class FeedbackForm extends React.Component<FeedbackFormProps> {

    handleSendFeedback = (values) => {
        const { sendFeedback, closeFeedbackModalDialog, productVariant, customAttribute } = this.props;

        sendFeedback(productVariant, customAttribute, values.feedback);
        closeFeedbackModalDialog();
    }

    render() {
        const { handleSubmit } = this.props;

        return <form id={FormName} noValidate onSubmit={handleSubmit(this.handleSendFeedback)}>
            <h4>Sdílení jiné zkušenosti, upozornění na chybu</h4>
            <Field name="feedback" type="textarea" component={ValidatedInputFormField} className="form-control" rows={10} required />
            <br />
            <span className="float-right">
                <Button color="primary" outline onClick={this.props.closeFeedbackModalDialog}>Zpět</Button>{" "}
                <Button color="primary" type="submit">Odeslat</Button>
            </span>
        </form>  
    }
}

const validate = (values) => {
    const errors = {};

    if ((values.feedback == undefined) || (values.feedback.length === 0)) {
        const errorMessage = required(values.feedback);
        if (errorMessage) {
            errors["feedback"] = errorMessage;
        }
    }

    return errors;
}

export default reduxForm({
    form: FormName,
    validate
})(FeedbackForm)