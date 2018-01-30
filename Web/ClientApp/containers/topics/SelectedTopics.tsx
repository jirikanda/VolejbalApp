import * as React from "react";
import { connect } from "react-redux";
import { Table } from "reactstrap";
import { ApplicationState } from "../../store/index";
import * as TopicStore from "../../store/Topics";
import { Topic } from '../../apimodels/TopicsStore';
import { SelectedTopicPanel } from "../../components/common/SelectedTopicPanel";
import { getAncestorsAndMe, INode } from "../../helpers/hierarchy";

type SelectedTopicsProps = TopicStore.TopicState &
    typeof TopicStore.actionCreators;

class SelectedTopics extends React.Component<SelectedTopicsProps> {
    render() {
        const { selectedTopics } = this.props;

        const removeButtonClick = (topic: Topic) => {
            this.props.unselectTopic(topic);
        };

        const selectedTopicPanels = selectedTopics.map((topic, index) =>
            <SelectedTopicPanel
                key={index}
                removeButtonClick={removeButtonClick}
                topics={getAncestorsAndMe(topic, this.props.topics)}
                isQuickSelection={false}
            />
        );

        return (
            <div>
                <b className="mb-2">Přehled vybraných témat</b>
                <Table hover striped className="topic-table">
                    <tbody>{selectedTopicPanels}</tbody>
                </Table>
            </div>
        );
    }
}

export default connect(
    (state: ApplicationState) => state.topics,
    TopicStore.actionCreators
)(SelectedTopics);
