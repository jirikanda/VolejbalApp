import * as React from 'react';
import { connect } from 'react-redux';
import * as SegmentStore from '../../store/Segments';
import * as ProductTypesStore from '../../store/ProductTypes';
import { ApplicationState } from '../../store';
import { NavLink, Link, withRouter, RouteComponentProps } from 'react-router-dom';
import SegmentsComponent from '../../components/segments/SegmentsComponent';

type SegmentsProps =
    SegmentStore.SegmentsState
    & typeof SegmentStore.actionCreators
    & typeof ProductTypesStore.actionCreators
    & RouteComponentProps<{}>;

export class Segments extends React.Component<SegmentsProps, {}> {
    componentWillMount() {
        // This method runs when the component is first added to the page
        this.props.requestSegments();
    }

    render() {

        if (this.props.segments === undefined) {
            return <span>Loading...</span>;
        }

        return <div className="container">
            <SegmentsComponent {...this.props} />
        </div>;
    }
}

// Wire up the React component to the Redux store
export default withRouter(connect((state: ApplicationState) => state.segments, {...ProductTypesStore.actionCreators, ...SegmentStore.actionCreators})(Segments));