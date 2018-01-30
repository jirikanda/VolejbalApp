import * as React from 'react';
import { Tooltip, UncontrolledTooltip } from 'reactstrap';
interface FeedbackButtonProps {
    id: string;
    feedback: string;
    feedbackButtonClicked: () => void;
}

const FeedbackButton = (props: FeedbackButtonProps) => {
    const { feedbackButtonClicked, feedback, id } = props;
    const feedbackButtonId = `feedback-button-${id}`;

    return <span id={feedbackButtonId} className="product-mtarix-item-feedback-button float-right" onClick={feedbackButtonClicked}>
        {feedback && <UncontrolledTooltip placement="top" target={feedbackButtonId}>
            <p>
                <b>Sdílená zkušenost - bez záruky</b>
            </p>
            {feedback}
        </UncontrolledTooltip>}
        <i className="fa fa-bell-o" aria-hidden="true">{feedback && <span className="fontawesome-badge">i</span>}</i>
    </span>;

}

export default FeedbackButton;