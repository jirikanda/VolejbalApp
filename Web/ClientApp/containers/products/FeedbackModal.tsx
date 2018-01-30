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
import FeedbackFrom from './FeedbackForm';

export const FormName = "FeedbackModalForm";

interface FeedbackModalProps {
    productVariant: ProductVariant;
    customAttribute: CustomAttribute;
    customAttributeAnswer: CustomAttributeValue;
    showFeedbackModalDialog: boolean;
    closeFeedbackModalDialog: () => void;
    ancestorTopicsAndMe: Topic[];
}

type FeedbackModalPropsPublic =
    ProductMatrixStore.ProductMatrixState
    & typeof ProductMatrixStore.actionCreators
    & FeedbackModalProps;

class FeedbackModal extends React.Component<FeedbackModalPropsPublic> {
    render() {
        const { customAttribute, productVariant, customAttributeAnswer, showFeedbackModalDialog, closeFeedbackModalDialog, ancestorTopicsAndMe } = this.props;

        const answerContent = getCustomAttributeAnswer(customAttributeAnswer);

        return <Modal dialogSize="lg"
            isOpen={showFeedbackModalDialog}
            title={productVariant.name}
            handleDialogClose={closeFeedbackModalDialog}>
            <div className="small text-secondary mb-1">
                <TopicPath topics={ancestorTopicsAndMe} />
            </div>
            <h4 className="mb-2">{customAttribute.name}</h4>
            <p>{answerContent}</p>
            {customAttributeAnswer.feedback &&
                <div>
                    <h4>Sdílení zkušeností - bez záruky</h4>
                    <p>{customAttributeAnswer.feedback}</p>
                </div>
            }
            <FeedbackFrom {...this.props} />
        </Modal>
    }
}

export default connect((state: ApplicationState, props: FeedbackModalProps) => state.productMatrix, ProductMatrixStore.actionCreators)(FeedbackModal)