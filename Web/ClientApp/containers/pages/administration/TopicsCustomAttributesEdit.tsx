import * as React from 'react';
import { Card, CardBody, CardGroup, CardTitle, Col, Row } from 'reactstrap';

import { GridRowType } from '../../../components/common/Treeview/GridInterfaces';
import ProductTypesPicker from '../../../containers/products/ProductTypesPicker';
import SegmentsPicker from '../../../containers/segments/SegmentsPicker';
import TopicsTreeview from '../../../containers/topics/administration/TopicsTreeview';
import TopicsCustomAttributesEditContainer from '../../../containers/topics/TopicsCustomAttributesEditContainer';

class TopicsCustomAttributesEdit extends React.Component {
    render() {
        return <div className="container-fluid">
            <h4>Editace témat a upřesňujících podmínek</h4>
            <Row>
                <Col md="4">
                    <CardGroup>
                        <Card>
                            <CardBody>
                                <CardTitle>
                                    Výběr segmentu
                                </CardTitle>
                                <SegmentsPicker />
                            </CardBody>
                        </Card>
                        <Card>
                            <CardBody>
                                <CardTitle>
                                    Výběr typu produktu
                                </CardTitle>
                                <ProductTypesPicker />
                            </CardBody>
                        </Card>
                    </CardGroup>
                </Col>
            </Row>
            <Row>
                <Col md="12">
                    <CardGroup>
                        <Card>
                            <CardBody>
                                <CardTitle>
                                    Výběr témat a upřesňujících podmínek
                                </CardTitle>
                                <TopicsTreeview />
                            </CardBody>
                        </Card>
                        <TopicsCustomAttributesEditContainer />
                    </CardGroup>
                </Col>
            </Row>
        </div>
    }
}

export default TopicsCustomAttributesEdit