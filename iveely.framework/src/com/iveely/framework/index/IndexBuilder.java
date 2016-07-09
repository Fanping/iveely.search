package com.iveely.framework.index;

import org.apache.lucene.analysis.Analyzer;
import org.apache.lucene.analysis.standard.StandardAnalyzer;
import org.apache.lucene.document.Document;
import org.apache.lucene.document.Field;
import org.apache.lucene.document.LongPoint;
import org.apache.lucene.document.StringField;
import org.apache.lucene.document.TextField;
import org.apache.lucene.index.IndexWriter;
import org.apache.lucene.index.IndexWriterConfig;
import org.apache.lucene.index.Term;
import org.apache.lucene.store.Directory;
import org.apache.lucene.store.FSDirectory;

import java.io.IOException;
import java.nio.file.Paths;
import java.util.List;

/**
 * author { liufanping@iveely.com }.
 */
public class IndexBuilder {

  protected IndexWriter writer;

  public IndexBuilder(final String indexPath) throws IOException {
    Analyzer analyzer = new StandardAnalyzer();
    IndexWriterConfig iwc = new IndexWriterConfig(analyzer);
    Directory dir = FSDirectory.open(Paths.get(indexPath));
    this.writer = new IndexWriter(dir, iwc);
    if (this.writer == null) {
      throw new IOException("Writer is null.");
    }
  }

  public void build(final StoreField<String> key, final List<StoreField> fields)
      throws
      IOException {
    Document doc = new Document();
    for (StoreField inputField : fields) {
      if (inputField.getType() == StoreField.Type.LONG) {
        doc.add(new LongPoint(inputField.getName(), (long) inputField
            .getValue()));
      } else if (inputField.getType() == StoreField.Type.STRING) {
        doc.add(new StringField(inputField.getName(),
            (String) inputField.getValue(), Field.Store.YES));
      } else {
        doc.add(new TextField(inputField.getName(), (String) inputField
            .getValue(), Field.Store.YES));
      }
    }
    if (writer.getConfig().getOpenMode() == IndexWriterConfig.OpenMode.CREATE) {
      // New index, so we just add the document (no old document can be there):
      writer.addDocument(doc);
    } else {
      // Existing index (an old copy of this document may have been indexed) so
      // we use updateDocument instead to replace the old one matching the exact
      // path, if present:
      writer.updateDocument(new Term(key.getName(), key.getValue()), doc);
    }
  }

  public void flush(boolean isClose) throws IOException {
    writer.flush();
    writer.commit();
    if (isClose) {
      writer.close();
    }
  }
}
