import * as React from 'react';
import { connect } from 'react-redux';
import * as SegmentStore from '../../store/Segments';
import { RouteComponentProps } from 'react-router-dom';
import { ApplicationState } from '../../store';

type SegmentNameProps =
    SegmentStore.SegmentsState
    & typeof SegmentStore.actionCreators
    & { segmentId?: number };

class SegmentName extends React.Component<SegmentNameProps> {
    componentWillMount() {
        if ((this.props.segment === null) || (this.props.segment.id !== this.props.segmentId)) {
            this.props.requestSegmentById(this.props.segmentId);
        }
    }

    render() {
        return this.props.segment && this.props.segment.name
    }
}

export default connect((state: ApplicationState, props: { segmentId?: number }) => state.segments, SegmentStore.actionCreators)(SegmentName)